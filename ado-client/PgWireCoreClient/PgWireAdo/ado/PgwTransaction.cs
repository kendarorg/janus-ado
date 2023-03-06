using System.Data;
using System.Data.Common;

namespace PgWireAdo.ado;

public class PgwTransaction:DbTransaction
{
    public PgwTransaction(PgwConnection pgwConnection, IsolationLevel isolationLevel)
    {
        DbConnection = pgwConnection;
        IsolationLevel = isolationLevel;
    }

    protected override DbConnection? DbConnection { get; }
    public override IsolationLevel IsolationLevel { get; }

    public override void Commit()
    {
        throw new NotImplementedException();
    }

    public override void Rollback()
    {
        throw new NotImplementedException();
    }
}