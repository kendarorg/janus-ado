using System.Text;
using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using PgWireAdo.ado;
using System.Data.Common;

namespace PgWireAdo.utils.parse;

public class SqlParser
{
    private static Regex namedParametersExpression = new Regex(@"([@|:]{1}[a-zA-Z0-9_]+)");
    private static Regex unnamedParametersExpression = new Regex(@"([?]{1})");
    private static Regex positionalParameterExpression = new Regex(@"([$]{1}[0-9]+)");


    static Dictionary<string, SqlStringType> select;
    static SqlParser()
    {
        select = new();
        select.Add("select", SqlStringType.SELECT);
        select.Add("show", SqlStringType.SELECT);
        select.Add("explain", SqlStringType.SELECT);
        select.Add("describe", SqlStringType.SELECT);
        select.Add("fetch", SqlStringType.SELECT);

        select.Add("update", SqlStringType.UPDATE);
        select.Add("insert", SqlStringType.INSERT);
        select.Add("delete", SqlStringType.UPDATE);
        select.Add("merge", SqlStringType.UPDATE);
        select.Add("alter", SqlStringType.UPDATE);
        select.Add("drop", SqlStringType.UPDATE);
        select.Add("grant", SqlStringType.UPDATE);
        select.Add("set", SqlStringType.UPDATE);
        select.Add("truncate", SqlStringType.UPDATE);
        select.Add("janus:", SqlStringType.UPDATE);

        select.Add("call", SqlStringType.CALL);
        select.Add("execute", SqlStringType.CALL);
        select.Add("run", SqlStringType.CALL);


        select.Add("create", SqlStringType.UNKNOWN);
        select.Add("declare", SqlStringType.UNKNOWN);
    }

    public static bool isUnknown(List<SqlParseResult> data)
    {
        foreach (SqlParseResult result in data)
        {
            if (result.Type == SqlStringType.UNKNOWN || result.Type == SqlStringType.NONE) return true;
        }

        return false;
    }

    public static List<SqlParseResult>? getTypes(string input)
    {
        var result = new List<SqlParseResult>();
        if (input == null) return result;
        var sqls = parseSql(input);
        foreach (var sql in sqls)
        {
            var splitted = Regex.Split(sql.Trim(), @"\s+");
            if (splitted.Length == 0)
            {
                continue;
            }
            var first = splitted[0].Trim().ToLowerInvariant();
            if (first.Length == 0)
            {
                continue;
            }
            if (select.ContainsKey(first))
            {
                var founded = select[first];
                result.Add(new SqlParseResult(sql, founded));
            }
            else
            {
                result.Add(new SqlParseResult(sql, SqlStringType.UNKNOWN));
            }
        }
        return result;
    }

    public static List<string> parseSql(string input)
    {
        List<string> sqls = new();
        var splitted = parseString(input);

        string tempValue = "";
        foreach (var line in splitted)
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("'") || trimmed.StartsWith("\""))
            {
                //Is a string
                tempValue += line;
                continue;
            }
            if (trimmed.Contains(";"))
            {
                foreach (var part in Regex.Split(line, @"((?<=;))"))
                {
                    var trimPart = part.Trim();
                    if (trimPart.EndsWith(";"))
                    {
                        tempValue += part;
                        sqls.Add(tempValue);
                        tempValue = "";
                    }
                    else
                    {
                        tempValue += part;
                    }
                }
                continue;
            }
            tempValue += line;
        }
        if (tempValue.Length > 0)
        {
            sqls.Add(tempValue);
        }
        return sqls;
    }
    public static List<string> parseString(string input)
    {
        List<string> tokens = new();
        int Length = input.Length;
        int i = 0;
        while (i < Length)
        {
            char c = input[i];
            if (c == '\'' || c == '\"')
            {
                StringBuilder sb = new StringBuilder();
                char delimiter = c;
                sb.Append(c); // include starting quote
                i++; // skip delimiter
                while (i < Length)
                {
                    c = input[i];
                    if (c == delimiter)
                    {
                        if (i + 1 < Length && input[i + 1] == delimiter && delimiter == '\'')
                        {
                            // Handle doubled delimiter
                            sb.Append(delimiter);
                            sb.Append(delimiter);
                            i += 2;
                        }
                        else
                        {
                            // Length of string
                            sb.Append(c);
                            i++;
                            break;
                        }
                    }
                    else if (c == '\\')
                    {
                        // Handle escaped character
                        sb.Append(c);
                        i++;
                        if (i < Length)
                        {
                            sb.Append(input[i]);
                            i++;
                        }
                    }
                    else
                    {
                        sb.Append(c);
                        i++;
                    }
                }
                tokens.Add(sb.ToString());
            }
            else
            {
                // Handle non-string token
                StringBuilder sb = new StringBuilder();
                while (i < Length)
                {
                    c = input[i];
                    if (c == '\'' || c == '\"')
                    {
                        break;
                    }
                    else
                    {
                        sb.Append(c);
                        i++;
                    }
                }
                tokens.Add(sb.ToString());
            }
        }
        return tokens;
    }


    public static bool isMixed(List<SqlParseResult> parsed)
    {
        SqlStringType founded = SqlStringType.NONE;
        foreach (var single in parsed)
        {
            if (founded == SqlStringType.NONE)
            {
                founded = single.Type;
            }
            if (single.Type != founded)
            {
                if (single.Type == SqlStringType.INSERT && founded == SqlStringType.UPDATE)
                {
                    continue;
                }
                else if (single.Type == SqlStringType.UPDATE && founded == SqlStringType.INSERT)
                {
                    continue;
                }
                return true;
            }

        }
        return false;
    }



    public static List<SqlParameter> getParameters(String input,out SqlParameterType type)
    {
        var sqlParameters = new List<SqlParameter>();
        var parsedString = parseString(input);
        type = SqlParameterType.NONE;
        var start = 0;
        for (var index = 0; index < parsedString.Count; index++)
        {
            var parsed = parsedString[index];
            if (parsed.StartsWith("'"))
            {
                start += parsed.Length;
                continue;
            }
            SqlParameterType internalType;
            var founded = retrieveParams(parsed, out internalType);
            foreach (var sqlParameter in founded)
            {
                sqlParameter.Start += start;
                sqlParameters.Add(sqlParameter);
            }
            if (internalType != SqlParameterType.NONE)
            {
                if (type != SqlParameterType.NONE && type != internalType)
                {
                    throw new InvalidOperationException("Cannot mix parameter styles");
                }
                type= internalType;
            }

            start += parsed.Length;
        }
        sqlParameters.Sort((x, y) => x.Start.CompareTo(y.Start));


        return sqlParameters;
    }

    private static IEnumerable<SqlParameter> retrieveParams(string input, out SqlParameterType type)
    {
        var sqlParameters = new List<SqlParameter>();
        type = SqlParameterType.NONE;
        var sqlParametersUnamed = new List<SqlParameter>();
        if (FindMatches(input, unnamedParametersExpression, sqlParametersUnamed, false))
        {
            type = SqlParameterType.UNNAMED;
            sqlParameters.AddRange(sqlParametersUnamed);


        }

        var sqlParametersPositional = new List<SqlParameter>();
        if (FindMatches(input, positionalParameterExpression, sqlParametersPositional, false))
        {
            if (type != SqlParameterType.NONE)
            {
                throw new InvalidOperationException("Cannot mix parameter styles");
            }
            type = SqlParameterType.POSITIONAL;
            sqlParameters.AddRange(sqlParametersPositional);
        }

        var sqlParametersNamed = new List<SqlParameter>();
        if (FindMatches(input, namedParametersExpression, sqlParametersNamed, true))
        {
            if (type != SqlParameterType.NONE)
            {
                throw new InvalidOperationException("Cannot mix parameter styles");
            }
            type = SqlParameterType.NAMED;
            sqlParameters.AddRange(sqlParametersNamed);
        }
        
        
        return sqlParameters;
    }

    private static bool FindMatches(string input, Regex expression, List<SqlParameter> sqlParameters,bool named)
    {
        var results = expression.Matches(input);
        foreach (Match match in results)
        {
            var val = match.Groups;
            var sqlParameter = new SqlParameter()
            {
                Named = named,
                Start = val[0].Index,
                Length = val[0].Length,
                Name = val[0].Value
            };
            sqlParameters.Add(sqlParameter);
        }

        return sqlParameters.Count > 0;
    }

    public static DbParameterCollection MaskParameters(ref string query, List<SqlParameter> parameters,
        DbParameterCollection dbParameterCollectionIn, SqlParameterType sqlParameterType)
    {
        var dbParameterCollection = (PgwParameterCollection)dbParameterCollectionIn;
        var originalQuery = query.ToString();
        if (sqlParameterType == SqlParameterType.UNNAMED) return dbParameterCollection;
        if (sqlParameterType == SqlParameterType.POSITIONAL) return dbParameterCollection;
        var queryResult = new StringBuilder();
        var newQueryParameters = new PgwParameterCollection();

        var lastIndex = 0;
        foreach (var sqlParameter in parameters)
        {
            var name = sqlParameter.Name;
            var src = (PgwParameter)dbParameterCollection[name];
            var previousQueryPart = originalQuery.Substring(lastIndex, sqlParameter.Start-lastIndex);
            queryResult.Append(previousQueryPart);
            queryResult.Append("?");
            newQueryParameters.Add(new PgwParameter()
            {
                Value = src.Value,
                DbType = src.DbType,
                Direction = src.Direction,
                IsNullable = src.IsNullable,
                Precision = src.Precision,
                Scale = src.Scale,
                Size = src.Size,
                ParameterName = src.ParameterName,
                SourceColumn = src.SourceColumn,
                SourceColumnNullMapping = src.SourceColumnNullMapping,
                SourceVersion = src.SourceVersion
            });
            lastIndex = sqlParameter.Start + sqlParameter.Length;
        }

        if (lastIndex! < originalQuery.Length)
        {
            queryResult.Append(originalQuery.Substring(lastIndex));
        }
        query = queryResult.ToString();

        return newQueryParameters;
    }
}