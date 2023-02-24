using System.Data.Common;
using PgWireAdo.ado;
using PgWireAdo.Test.Utils;


namespace Npgsql.Tests;

    public class SimpleTests : TestBase
    {

        [Test]
        public void Test1()
        {
            var conn = OpenConnection();
           var  cmd = conn.CreateCommand();
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


            //conn.Close();
        }
    }
