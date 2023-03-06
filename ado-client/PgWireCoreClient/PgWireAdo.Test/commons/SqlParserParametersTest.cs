using System.Data;
using PgWireAdo.utils.parse;
using System.Data.Common;
using PgWireAdo.ado;

namespace Npgsql.Tests;

public class SqlParserParametersTest
{
    [Test]
    public void onlyParDeclaration()
    {
        var query = "@p1";
        SqlParameterType parametersType;
        var parameters = new PgwParameterCollection();
        parameters.Add(new PgwParameter("p1", DbType.String) { Value = "test" });
        var foundedParameters = SqlParser.getParameters(query, out parametersType);
        var parametersCollection = SqlParser.MaskParameters(ref query, foundedParameters, parameters, parametersType);

        Assert.AreEqual("?", query);
    }

    [Test]
    public void surrounded()
    {
        var query = "A=@p1+B";
        SqlParameterType parametersType;
        var parameters = new PgwParameterCollection();
        parameters.Add(new PgwParameter("p1", DbType.String) { Value = "test" });
        var foundedParameters = SqlParser.getParameters(query, out parametersType);
        var parametersCollection = SqlParser.MaskParameters(ref query, foundedParameters, parameters, parametersType);

        Assert.AreEqual("A=?+B", query);
    }

    [Test]
    public void followString()
    {
        var query = "A=@p1's' a";
        SqlParameterType parametersType;
        var parameters = new PgwParameterCollection();
        parameters.Add(new PgwParameter("p1", DbType.String) { Value = "test" });
        var foundedParameters = SqlParser.getParameters(query, out parametersType);
        var parametersCollection = SqlParser.MaskParameters(ref query, foundedParameters, parameters, parametersType);

        Assert.AreEqual("A=?'s' a", query);
    }

    [Test]
    public void preceedString()
    {
        var query = "A='s'@p1+a";
        SqlParameterType parametersType;
        var parameters = new PgwParameterCollection();
        parameters.Add(new PgwParameter("p1", DbType.String) { Value = "test" });
        var foundedParameters = SqlParser.getParameters(query, out parametersType);
        var parametersCollection = SqlParser.MaskParameters(ref query, foundedParameters, parameters, parametersType);

        Assert.AreEqual("A='s'?+a", query);
    }

    [Test]
    public void other()
    {
        //var query = "SELECT * FROM @p1 WHERE P='%' A=:p2;";
        var query = "SELECT * FROM table WHERE A='' AND B=@p1 ORDER BY TEST;";
        SqlParameterType parametersType;
        var parameters = new PgwParameterCollection();
        parameters.Add(new PgwParameter("p1", DbType.String) { Value = "test" });
        var foundedParameters = SqlParser.getParameters(query, out parametersType);
        var parametersCollection = SqlParser.MaskParameters(ref query, foundedParameters, parameters, parametersType);

        Assert.AreEqual("SELECT * FROM table WHERE A='' AND B=? ORDER BY TEST;",query);
    }



    [Test]
    public void multipleWithSameName()
    {
        //var query = "SELECT * FROM @p1 WHERE P='%' A=:p2;";
        var query = "SELECT * FROM table WHERE A='' AND B=@p1 OR C=@p1 ORDER BY TEST;";
        SqlParameterType parametersType;
        var parameters = new PgwParameterCollection();
        parameters.Add(new PgwParameter("p1", DbType.String) { Value = "test" });
        var foundedParameters = SqlParser.getParameters(query, out parametersType);
        var parametersCollection = SqlParser.MaskParameters(ref query, foundedParameters, parameters, parametersType);

        Assert.AreEqual("SELECT * FROM table WHERE A='' AND B=? OR C=? ORDER BY TEST;", query);
        Assert.AreEqual(2, parametersCollection.Count);
        Assert.AreEqual("test", parametersCollection[0].Value);
        Assert.AreEqual("test", parametersCollection[1].Value);
    }

    [Test]
    public void mixedStuffs()
    {
        //var query = "SELECT * FROM @p1 WHERE P='%' A=:p2;";
        var query = "SELECT * FROM table WHERE Z=@p2 A='' AND B=@p1 OR D='fo' C=@p1 ORDER BY TEST;";
        SqlParameterType parametersType;
        var parameters = new PgwParameterCollection();
        parameters.Add(new PgwParameter("p1", DbType.String) { Value = "test" });
        parameters.Add(new PgwParameter("p2", DbType.String) { Value = "toast" });
        var foundedParameters = SqlParser.getParameters(query, out parametersType);
        var parametersCollection = SqlParser.MaskParameters(ref query, foundedParameters, parameters, parametersType);

        Assert.AreEqual("SELECT * FROM table WHERE Z=? A='' AND B=? OR D='fo' C=? ORDER BY TEST;", query);
        Assert.AreEqual(3, parametersCollection.Count);
        Assert.AreEqual("toast", parametersCollection[0].Value);
        Assert.AreEqual("test", parametersCollection[1].Value);
        Assert.AreEqual("test", parametersCollection[2].Value);
    }
}