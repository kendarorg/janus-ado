using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgWireAdo.ado;
public class PgwProviderFactory: DbProviderFactory
{
    static PgwProviderFactory()
    {
        DbProviderFactories.RegisterFactory("PgwProvider", PgwProviderFactory.Instance);
    }

    public static readonly PgwProviderFactory Instance = new PgwProviderFactory();
    public override DbConnection? CreateConnection()
    {
        return new PgwConnection();
    }
}
