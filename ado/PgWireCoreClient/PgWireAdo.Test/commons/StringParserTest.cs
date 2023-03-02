using PgWireAdo.utils;

namespace Npgsql.Tests;

public class StringParserTest
{
    [Test]
    public void quotesHandling()
    {
        var result = StringParser.parseString("Example 'te''st' simple");
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("Example ", result[0]);
        Assert.AreEqual("'te''st'", result[1]);
        Assert.AreEqual(" simple", result[2]);

        result = StringParser.parseString("Example 'test' simple");
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("Example ", result[0]);
        Assert.AreEqual("'test'", result[1]);
        Assert.AreEqual(" simple", result[2]);

        result = StringParser.parseString("Example 'te\\'st' simple");
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("Example ", result[0]);
        Assert.AreEqual("'te\\'st'", result[1]);
        Assert.AreEqual(" simple", result[2]);

        result = StringParser.parseString("Example \"test\" simple");
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("Example ", result[0]);
        Assert.AreEqual("\"test\"", result[1]);
        Assert.AreEqual(" simple", result[2]);

        result = StringParser.parseString("Example \"te\\\"st\" simple");
        Assert.AreEqual(3, result.Count);
        Assert.AreEqual("Example ", result[0]);
        Assert.AreEqual("\"te\\\"st\"", result[1]);
        Assert.AreEqual(" simple", result[2]);
    }

    [Test]
    public void semicolumnHandling()
    {
        var result = StringParser.parseSql("SELECT 1;UPDATE test SET name='y''o' WHERE 1=0;");
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("SELECT 1;", result[0]);
        Assert.AreEqual("UPDATE test SET name='y''o' WHERE 1=0;", result[1]);

        result = StringParser.parseSql("SELECT 1;UPDATE test SET name='y''o' WHERE 1=0");
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("SELECT 1;", result[0]);
        Assert.AreEqual("UPDATE test SET name='y''o' WHERE 1=0", result[1]);
    }

    [Test]
    public void standardQuery()
    {
        var result = StringParser.getTypes("SELECT 1;UPDATE test SET name='y''o' WHERE 1=0;");
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("SELECT 1;", result[0].Value);
        Assert.AreEqual(SqlStringType.SELECT, result[0].Type);
        Assert.AreEqual("UPDATE test SET name='y''o' WHERE 1=0;", result[1].Value);
        Assert.AreEqual(SqlStringType.UPDATE, result[1].Type);
    }

    [Test]
    public void withError()
    {
        var result = StringParser.getTypes("\r\nDROP TABLE IF EXISTS temp_table1 CASCADE;\r\n CREATE TABLE temp_table1 (intf int);\r\n");
        Assert.AreEqual(2, result.Count);
        Assert.AreEqual("\r\nDROP TABLE IF EXISTS temp_table1 CASCADE;", result[0].Value);
        Assert.AreEqual(SqlStringType.UPDATE, result[0].Type);
        Assert.AreEqual("\r\n CREATE TABLE temp_table1 (intf int);", result[1].Value);
        Assert.AreEqual(SqlStringType.UNKNOWN, result[1].Type);
    }
}