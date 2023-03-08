using PgWireAdo.utils.parse;
using System.Text.RegularExpressions;

namespace Npgsql.Tests;

public class SqlParserTest
{
    [Test]
    public void quotesHandling()
    {
        var result = SqlParser.parseString("Example 'te''st' simple");
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("Example ", result[0]);
        Assert.AreEqual("'te''st'", result[1]);
        Assert.AreEqual(" simple", result[2]);

        result = SqlParser.parseString("Example 'test' simple");
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("Example ", result[0]);
        Assert.AreEqual("'test'", result[1]);
        Assert.AreEqual(" simple", result[2]);

        result = SqlParser.parseString("Example 'te\\'st' simple");
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("Example ", result[0]);
        Assert.AreEqual("'te\\'st'", result[1]);
        Assert.AreEqual(" simple", result[2]);

        result = SqlParser.parseString("Example \"test\" simple");
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("Example ", result[0]);
        Assert.AreEqual("\"test\"", result[1]);
        Assert.AreEqual(" simple", result[2]);

        result = SqlParser.parseString("Example \"te\\\"st\" simple");
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("Example ", result[0]);
        Assert.AreEqual("\"te\\\"st\"", result[1]);
        Assert.AreEqual(" simple", result[2]);
    }

    [Test]
    public void semicolumnHandling()
    {
        var result = SqlParser.parseSql("SELECT 1;UPDATE test SET name='y''o' WHERE 1=0;");
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("SELECT 1;", result[0]);
        Assert.AreEqual("UPDATE test SET name='y''o' WHERE 1=0;", result[1]);

        result = SqlParser.parseSql("SELECT 1;UPDATE test SET name='y''o' WHERE 1=0");
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("SELECT 1;", result[0]);
        Assert.AreEqual("UPDATE test SET name='y''o' WHERE 1=0", result[1]);
    }

    [Test]
    public void standardQuery()
    {
        var result = SqlParser.getTypes("SELECT 1;UPDATE test SET name='y''o' WHERE 1=0;");
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("SELECT 1;", result[0].Value);
        Assert.AreEqual(SqlStringType.SELECT, result[0].Type);
        Assert.AreEqual("UPDATE test SET name='y''o' WHERE 1=0;", result[1].Value);
        Assert.AreEqual(SqlStringType.UPDATE, result[1].Type);
    }

    [Test]
    public void withError()
    {
        var result = SqlParser.getTypes("\r\nDROP TABLE IF EXISTS temp_table1 CASCADE;\r\n CREATE TABLE temp_table1 (intf int);\r\n");
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("\r\nDROP TABLE IF EXISTS temp_table1 CASCADE;", result[0].Value);
        Assert.AreEqual(SqlStringType.UPDATE, result[0].Type);
        Assert.AreEqual("\r\n CREATE TABLE temp_table1 (intf int);", result[1].Value);
        Assert.AreEqual(SqlStringType.UNKNOWN, result[1].Type);
    }

    [Test]
    public void verifyRegexpNamed()
    {
        var sqlParameters = new List<SqlParameter>();
        var input = "select @p1 and ? and $1 or :p2";
        Regex namedParametersExpression = new Regex(@"([@|:]{1}[a-zA-Z0-9_\-]+)");
        var results = namedParametersExpression.Matches(input);
        foreach (Match match in results)
        {
            var val = match.Groups;
            var sqlParameter = new SqlParameter()
            {
                Named = true,
                Start = val[0].Index,
                Length = val[0].Length,
                Name = val[0].Value
            };
            sqlParameters.Add(sqlParameter);
            Console.WriteLine(sqlParameter);
        }
        Assert.AreEqual(2,sqlParameters.Count);
    }



    [Test]
    public void verifyRegexpPositional()
    {
        var sqlParameters = new List<SqlParameter>();
        var input = "select @p1 and ? and $1 or $2 and :p2";
        Regex positionalParameterExpression = new Regex(@"([$]{1}[0-9]+)");
        var results = positionalParameterExpression.Matches(input);
        foreach (Match match in results)
        {
            var val = match.Groups;
            var sqlParameter = new SqlParameter()
            {
                Named = true,
                Start = val[0].Index,
                Length = val[0].Length,
                Name = val[0].Value
            };
            sqlParameters.Add(sqlParameter);
            Console.WriteLine(sqlParameter);
        }
        Assert.AreEqual(2, sqlParameters.Count);
    }

    [Test]
    public void verifyRegexpPositionalUnnamed()
    {
        var sqlParameters = new List<SqlParameter>();
        var input = "select @p1 and ? and $1 or $2 and :p2";
        Regex unnamedParametersRegexp = new Regex(@"([?]{1})");
        var results = unnamedParametersRegexp.Matches(input);
        foreach (Match match in results)
        {
            var val = match.Groups;
            var sqlParameter = new SqlParameter()
            {
                Named = true,
                Start = val[0].Index,
                Length = val[0].Length,
                Name = val[0].Value
            };
            sqlParameters.Add(sqlParameter);
            Console.WriteLine(sqlParameter);
        }
        Assert.AreEqual(1, sqlParameters.Count);
    }

    [Test]
    public void verifyRegexpNamedMultiple()
    {
        var sqlParameters = new List<SqlParameter>();
        var input = "select @p1 and @p1";
        Regex namedParametersExpression = new Regex(@"([@|:]{1}[a-zA-Z0-9_\-]+)");
        var results = namedParametersExpression.Matches(input);
        foreach (Match match in results)
        {
            var val = match.Groups;
            var sqlParameter = new SqlParameter()
            {
                Named = true,
                Start = val[0].Index,
                Length = val[0].Length,
                Name = val[0].Value
            };
            sqlParameters.Add(sqlParameter);
            Console.WriteLine(sqlParameter);
        }
        Assert.AreEqual(2, sqlParameters.Count);
    }

    /*[Test]
    public void retrieveParametersUnnamedPriority()
    {
        var sql = "SELECT ?,@p1,$1";
        SqlParameterType parametersType;
        var result = SqlParser.getParameters(sql,out parametersType);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(SqlParameterType.UNNAMED, parametersType);
        Assert.AreEqual("?", result[0].Name);
    }

    [Test]
    public void retrieveParametersPositionlPriority()
    {
        var sql = "SELECT @p1,$1";
        SqlParameterType parametersType;
        var result = SqlParser.getParameters(sql, out parametersType);
        Assert.AreEqual(1, result.Count);
        Assert.AreEqual(SqlParameterType.POSITIONAL, parametersType);
        Assert.AreEqual("$1", result[0].Name);
    }*/



    [Test]
    public void retrieveParametersNamedPriority()
    {
        var sql = "SELECT @p1,:p2";
        SqlParameterType parametersType;
        var result = SqlParser.getParameters(sql, out parametersType);
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual(SqlParameterType.NAMED, parametersType);
        Assert.AreEqual("@p1", result[0].Name);
    }

    [Test]
    public void errorMixingParameterTypes()
    {
        var sql = "SELECT ?,' ',@p1,$1";
        SqlParameterType parametersType;
        Assert.Throws<InvalidOperationException>(() => SqlParser.getParameters(sql, out parametersType));
    }
}