using NUnit.Framework;
using System;
using System.Data;
using System.Data.Odbc;

namespace ODBCConnection
{
    /*
     * CREATE TABLE vbtest(
    id serial,
    data text,
    accessed timestamp
);
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
            OdbcConnection cnDB = new OdbcConnection(szConnect);

            // The following code demonstrates how to catch & report an ODBC exception.
            // To keep things simple, this is the only exception handling in this example.
            // Note: The ODBC data provider requests ODBC3 from the driver. At the time of
            //       writing, the psqlODBC driver only supports ODBC2.5 - this will cause
            //       an additional error, but will *not* throw an exception.
            try
            {
                cnDB.Open();
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
            DataSet dsDB = new DataSet();
            OdbcDataAdapter adDB = new OdbcDataAdapter();
            OdbcCommandBuilder cbDB = new OdbcCommandBuilder(adDB);
            adDB.SelectCommand = new OdbcCommand(
                "SELECT id, data, accessed FROM csharptest",
                cnDB);
            adDB.Fill(dsDB);

            // Display the record count
            Console.WriteLine("Table 'csharptest' contains {0} rows.\n",
                dsDB.Tables[0].Rows.Count);

            // List the columns (using a foreach loop)
            Console.WriteLine("Columns\n=======\n");

            foreach (DataColumn dcDB in dsDB.Tables[0].Columns)
                Console.WriteLine("{0} ({1})", dcDB.ColumnName, dcDB.DataType);
            Console.WriteLine("\n");

            // Iterate through the rows and display the data in the table (using a for loop).
            // Display the data column last for readability.
            Console.WriteLine("Data\n====\n");
            //TODO for (int i = 0; i to continue...");
            Console.Read();
        }
    }
}