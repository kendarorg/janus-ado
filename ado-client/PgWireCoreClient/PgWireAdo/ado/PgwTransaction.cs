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
        ((PgwConnection)DbConnection).Commit();
    }

    public override void Rollback()
    {
        ((PgwConnection)DbConnection).Rollback();
    }
}