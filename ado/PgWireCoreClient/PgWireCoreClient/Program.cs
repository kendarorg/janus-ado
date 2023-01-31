// See https://aka.ms/new-console-template for more information

using System.Data;
using System.Data.Common;
using PgWireAdo.ado;

DbProviderFactories.RegisterFactory("Npgsql", Npgsql.NpgsqlFactory.Instance);

using (DataTable providers = DbProviderFactories.GetFactoryClasses())
{
    Console.WriteLine("Available ADO.NET Data Providers:");
    foreach (DataRow prov in providers.Rows)
    {
        Console.WriteLine("Name:{0}", prov["Name"]);
        Console.WriteLine("Description:{0}", prov["Description"]);
        Console.WriteLine("Invariant Name:{0}", prov["InvariantName"]);
    }
}

DbProviderFactory factory = PgwProviderFactory.Instance;
DbConnection conn = factory.CreateConnection();
conn.ConnectionString = "Host=localhost; Database=test;";
conn.Open();
conn.Close();

Console.WriteLine("DONE!");
/*

DbProviderFactory factory = DbProviderFactories.GetFactory("Npgsql");
DbConnection conn = factory.CreateConnection();
conn.ConnectionString = "Host=localhost; Database=test; Username=sa;Password=sa;SSL Mode=DISABLE;";
conn.Open();
var cmd = conn.CreateCommand();
cmd.CommandText = "create table if not exists test(id int, name varchar)";
cmd.ExecuteNonQuery();

cmd = conn.CreateCommand();
cmd.CommandText = "insert into test values(1,'fuffa')";
cmd.ExecuteNonQuery();

cmd = conn.CreateCommand();
cmd.CommandText = "insert into test values(2,'boffa')";
cmd.ExecuteNonQuery();

cmd = conn.CreateCommand();
cmd.CommandText = "select * FROM test";
var reader = cmd.ExecuteReader();
if (reader.HasRows)
{
    while (reader.Read())
    {
        Console.WriteLine("{0}\t{1}", reader.GetInt32(0),
            reader.GetString(1));
    }
}
*/