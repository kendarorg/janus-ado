using System.Data.Common;
using PgWireAdo.ado;
using PgWireAdo.Test.Utils;
using PgWireAdo.utils;


namespace Npgsql.Tests;

public class SimpleTests : TestBase
{

    [Test]
    public void Test1()
    {
        ConsoleOut.setup((String a) => { TestContext.Out.WriteLine(a); });
        using (var conn = OpenConnection())
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = "drop table if  exists test";
            cmd.ExecuteNonQuery();
            cmd = conn.CreateCommand();
            cmd.CommandText = "create table if not exists test(id int, name varchar)";
            cmd.ExecuteNonQuery();

            cmd = conn.CreateCommand();
            cmd.CommandText = "insert into test values(1,'test1')";
            cmd.ExecuteNonQuery();

            cmd = conn.CreateCommand();
            cmd.CommandText = "insert into test values(2,'test2')";
            cmd.ExecuteNonQuery();

            cmd = conn.CreateCommand();
            cmd.CommandText = "select * FROM test";
            var reader = cmd.ExecuteReader();
            Assert.True(reader.HasRows);
            Assert.True(reader.Read());
            Assert.AreEqual(1, reader.GetInt32(0));
            Assert.AreEqual("test1", reader.GetString(1));
            Assert.True(reader.Read());
            Assert.AreEqual(2, reader.GetInt32(0));
            Assert.AreEqual("test2", reader.GetString(1));


        }
    }



    [Test]
    public void TestWithPs()
    {
        using (var conn = OpenConnection())
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = "drop table if  exists test";
            cmd.ExecuteNonQuery();
            cmd = conn.CreateCommand();
            cmd.CommandText = "create table if not exists test(id int, name varchar)";
            cmd.ExecuteNonQuery();

            cmd = conn.CreateCommand();
            cmd.CommandText = "insert into test values(1,'test1')";
            cmd.ExecuteNonQuery();

            cmd = conn.CreateCommand();
            cmd.CommandText = "insert into test values(2,'test2')";
            cmd.ExecuteNonQuery();

            cmd = conn.CreateCommand();
            cmd.CommandText = "select * FROM test where name=@val1";
            cmd.Parameters.AddWithValue("@val1", "test2");
            var reader = cmd.ExecuteReader();
            Assert.True(reader.HasRows);
            Assert.True(reader.Read());
            Assert.AreEqual(2, reader.GetInt32(0));
            Assert.AreEqual("test2", reader.GetString(1));
            Assert.False(reader.Read());
        }
    }

    [Test]
    public void TestSlowness()
    {
        ConsoleOut.setup((String a) => { TestContext.Out.WriteLine(a); });
        using (var conn = OpenConnection())
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = "drop table if  exists test";
            cmd.ExecuteNonQuery();
        }
    }

    [Test]
    public void TestUTF()
    {
        ConsoleOut.setup((String a) => { TestContext.Out.WriteLine(a); });
        using (var conn = OpenConnection())
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = "drop table if  exists test";
            cmd.ExecuteNonQuery();
            cmd = conn.CreateCommand();
            cmd.CommandText = "create table if not exists test(id int, name varchar)";
            cmd.ExecuteNonQuery();

            cmd = conn.CreateCommand();
            cmd.CommandText = "insert into test values(1,'∮ E⋅da = Q,  n → ∞, ∑ f(i)')";
            cmd.ExecuteNonQuery();

            cmd = conn.CreateCommand();
            cmd.CommandText = "select * FROM test";
            var reader = cmd.ExecuteReader();
            Assert.True(reader.HasRows);
            Assert.True(reader.Read());
            Assert.AreEqual(1, reader.GetInt32(0));
            Assert.AreEqual("∮ E⋅da = Q,  n → ∞, ∑ f(i)", reader.GetString(1));
            Assert.False(reader.Read());


        }
    }

    [Test]
    public void Rollback()
    {
        using (var conn = OpenConnection())
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = "drop table if  exists test";
            cmd.ExecuteNonQuery();
            cmd = conn.CreateCommand();
            cmd.CommandText = "create table if not exists test(id int, name varchar)";
            cmd.ExecuteNonQuery();
            using var t = conn.BeginTransaction();

            for (var x = 0; x < 5; x++)
                conn.ExecuteNonQuery($"INSERT INTO test (name) VALUES ('X')");
            t.Rollback();

            cmd = conn.CreateCommand();
            cmd.CommandText = "select count(*) from test";
            var reader = cmd.ExecuteReader();
            Assert.True(reader.HasRows);
            Assert.True(reader.Read());
            Assert.AreEqual(0, reader.GetInt32(0));


        }
    }

    [Test]
    public void Commit()
    {
        using (var conn = OpenConnection())
        {
            var cmd = conn.CreateCommand();
            cmd.CommandText = "drop table if  exists test";
            cmd.ExecuteNonQuery();
            cmd = conn.CreateCommand();
            cmd.CommandText = "create table if not exists test(id int, name varchar)";
            cmd.ExecuteNonQuery();
            using var t = conn.BeginTransaction();

            for (var x = 0; x < 5; x++)
                conn.ExecuteNonQuery($"INSERT INTO test (name) VALUES ('X')");
            t.Commit();

            cmd = conn.CreateCommand();
            cmd.CommandText = "select count(*) from test";
            var reader = cmd.ExecuteReader();
            Assert.True(reader.HasRows);
            Assert.True(reader.Read());
            Assert.AreEqual(5, reader.GetInt32(0));


        }
    }
}
