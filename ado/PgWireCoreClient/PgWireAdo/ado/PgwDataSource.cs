namespace PgWireAdo.ado;

using System.Data.Common;

public class PgwDataSource:IDisposable,IAsyncDisposable{
    public string ConnectionString { get; }

    private PgwDataSource(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public static PgwDataSource Create(string connectionString)
    {
        return new PgwDataSource(connectionString);
    }

    public void Dispose()
    {
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }

    public async Task<DbConnection> OpenConnectionAsync()
    {
        return new PgwConnection() { ConnectionString = ConnectionString };
    }
}