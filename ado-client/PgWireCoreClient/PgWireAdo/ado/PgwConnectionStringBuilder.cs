namespace PgWireAdo.ado;

public class PgwConnectionStringBuilder
{
    public PgwConnectionStringBuilder()
    {
        CommandTimeout = 10000;
    }

    public PgwConnectionStringBuilder(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public string ConnectionString { get; set; }

    public int CommandTimeout { get; set; }
}