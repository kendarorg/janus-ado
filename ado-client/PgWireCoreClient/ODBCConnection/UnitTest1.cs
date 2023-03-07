using NUnit.Framework;
using System;
using System.Data;
using System.Data.Odbc;
using System.Data.SqlClient;

namespace ODBCConnection
{
    /*
     * CREATE TABLE vbtest(id serial,data text,accessed timestamp);
INSERT INTO csharptest(data, accessed) VALUES('Rows: 1', now());
INSERT INTO csharptest(data, accessed) VALUES('Rows: 2', now());
INSERT INTO csharptest(data, accessed) VALUES('Rows: 3', now());
     */
    public class Tests
    {

        [Test]
        public void Test1()
        {
            // Setup a connection string
            string szConnect = "DSN=localpostgres;" +
                               "UID=postgres;" +
                               "PWD=postgres;CommLog=1";

            // Attempt to open a connection
            OdbcConnection conn = new OdbcConnection(szConnect);

            // The following code demonstrates how to catch & report an ODBC exception.
            // To keep things simple, this is the only exception handling in this example.
            // Note: The ODBC data provider requests ODBC3 from the driver. At the time of
            //       writing, the psqlODBC driver only supports ODBC2.5 - this will cause
            //       an additional error, but will *not* throw an exception.
            try
            {
                conn.Open();
            }
            catch (OdbcException ex)
            {
                Console.WriteLine(ex.Message + "\n\n" + "StackTrace: \n\n" + ex.StackTrace);
                // Pause for the user to read the screen.
                Console.WriteLine("\nPress  to continue...");
                Console.Read();
                return;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + "\n\n" + "StackTrace: \n\n" + ex.StackTrace);
                // Pause for the user to read the screen.
                Console.WriteLine("\nPress  to continue...");
                Console.Read();
                return;
            }

            // Create a dataset
            new OdbcCommand("drop table if exists csharptest;", conn)
                .ExecuteNonQuery();
            new OdbcCommand("CREATE TABLE csharptest(id serial,data text,accessed timestamp);", conn)
                .ExecuteNonQuery();

            new OdbcCommand("INSERT INTO csharptest(data, accessed) VALUES('Rows: 1', now());", conn)
                .ExecuteNonQuery();
            new OdbcCommand("INSERT INTO csharptest(data, accessed) VALUES('Rows: 2', now());", conn)
                .ExecuteNonQuery();
            new OdbcCommand("INSERT INTO csharptest(data, accessed) VALUES('Rows: 3', now());", conn)
                .ExecuteNonQuery();




            DataSet dsDB = new DataSet();
            OdbcDataAdapter adDB = new OdbcDataAdapter();
            OdbcCommandBuilder cbDB = new OdbcCommandBuilder(adDB);
           


            adDB.SelectCommand = new OdbcCommand(
                "SELECT id, data, accessed FROM csharptest",
                conn);
            adDB.Fill(dsDB);

            // Display the record count
            Assert.AreEqual(3,
                dsDB.Tables[0].Rows.Count);


            Console.WriteLine("Rows\n=======\n");
            foreach (DataRow row in dsDB.Tables[0].Rows)
            {
                var tmp = row[0]+"|" + row[1]+"|"+row[2];
                Console.WriteLine(" "+tmp);

            }

            // List the columns (using a foreach loop)
            Console.WriteLine("Columns\n=======\n");

            foreach (DataColumn dcDB in dsDB.Tables[0].Columns)
                Console.WriteLine("{0} ({1})", dcDB.ColumnName, dcDB.DataType);
            Console.WriteLine("\n");

            conn.Close();
        }

        [Test]
        public void Rollback()
        {
            string szConnect = "DSN=localpostgres;" +
                               "UID=postgres;" +
                               "PWD=postgres;CommLog=1";
            OdbcConnection conn = new OdbcConnection(szConnect);

            conn.Open();


            // Create a dataset

            new OdbcCommand("drop table if exists csharptest;", conn)
                .ExecuteNonQuery();
            new OdbcCommand("CREATE TABLE csharptest(id serial,data text,accessed timestamp);", conn)
                .ExecuteNonQuery();

            conn = new OdbcConnection(szConnect);
            conn.Open();

            using (OdbcCommand cmd = conn.CreateCommand())
            {
                var transaction = conn.BeginTransaction();

                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = "INSERT INTO csharptest(data, accessed) VALUES('Rows: 1', now());";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "INSERT INTO csharptest(data, accessed) VALUES('Rows: 2', now());";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "INSERT INTO csharptest(data, accessed) VALUES('Rows: 3', now());";
                cmd.ExecuteNonQuery();
                transaction.Rollback();

            }
            conn = new OdbcConnection(szConnect);
            conn.Open();
            DataSet dsDB = new DataSet();
            OdbcDataAdapter adDB = new OdbcDataAdapter();
            OdbcCommandBuilder cbDB = new OdbcCommandBuilder(adDB);


            var sel = new OdbcCommand(
                "SELECT id, data, accessed FROM csharptest",
                conn);
            adDB.SelectCommand = sel;
            adDB.Fill(dsDB);

            Assert.AreEqual(0,
                dsDB.Tables[0].Rows.Count);

            conn.Close();
        }



        [Test]
        public void Commit()
        {
            string szConnect = "DSN=localpostgres;" +
                               "UID=postgres;" +
                               "PWD=postgres;COMMANDTIMEOUT=20;";
            OdbcConnection conn = new OdbcConnection(szConnect);

            conn.Open();


            // Create a dataset

            new OdbcCommand("drop table if exists csharptest;", conn)
                .ExecuteNonQuery();
            new OdbcCommand("CREATE TABLE csharptest(id serial,data text,accessed timestamp);", conn)
                .ExecuteNonQuery();



            OdbcCommand cmd = conn.CreateCommand();
            {
                var transaction = conn.BeginTransaction();
                
                cmd.Connection = conn;
                cmd.Transaction = transaction;
                cmd.CommandText = "INSERT INTO csharptest(data, accessed) VALUES('Rows: 1', now());";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "INSERT INTO csharptest(data, accessed) VALUES('Rows: 2', now());";
                cmd.ExecuteNonQuery();
                cmd.CommandText = "INSERT INTO csharptest(data, accessed) VALUES('Rows: 3', now());";
                cmd.ExecuteNonQuery();
                transaction.Commit();
            
            }
            //conn = new OdbcConnection(szConnect);
            //conn.Open();
            DataSet dsDB = new DataSet();
            OdbcDataAdapter adDB = new OdbcDataAdapter();


            conn = new OdbcConnection(szConnect);
conn.Open();
            var sel = new OdbcCommand(
                "SELECT id, data, accessed FROM csharptest",
                conn);
            adDB.SelectCommand = sel;
            adDB.Fill(dsDB);

            Assert.AreEqual(3,
                dsDB.Tables[0].Rows.Count);

            conn.Close();
        }
    }
}