using System.Text;
using System;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace PgWireAdo.utils;

public class StringParser
{
    static Dictionary<string, SqlStringType> select;
    static StringParser()
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
                foreach (var part in Regex.Split(line,@"((?<=;))"))
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
                            // End of string
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
}