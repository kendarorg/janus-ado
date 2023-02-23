using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PgWireAdo.ado;
using PgWireAdo.utils;

namespace PgWireAdo.Test.Utils
{
    public static class TestUtils
    {

        private static int _tempTableCounter = 0;


        public static async Task<string> CreateTempTable(DbConnection conn, string columns)
        {
            var tableName = "temp_table" + Interlocked.Increment(ref _tempTableCounter);

            await conn.ExecuteNonQueryAsync(@$"
DROP TABLE IF EXISTS {tableName} CASCADE;
CREATE TABLE {tableName} ({columns});");

            return tableName;
        }

       

        
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class IssueLink : Attribute
{
    public string LinkAddress { get; private set; }
    public IssueLink(string linkAddress)
    {
        LinkAddress = linkAddress;
    }
}

public class NpgsqlParameter:PgwParameter{
        public NpgsqlParameter(string parameterName, Object? dbType)
        {
            this.ParameterName = parameterName;
            if (dbType != null)
            {
                if (typeof(DbType) == dbType.GetType()) DbType = (DbType)dbType;
                else Value = dbType;
            }
        }

        public NpgsqlParameter()
        {
            
        }
}

    public class NpgsqlParameter<T> : PgwParameter<T>
    {
        public NpgsqlParameter(string parameterName, T? value) : base(parameterName, value)
        {
            this.ParameterName = parameterName;
            this.Value = value;
        }
    }

    public enum PrepareOrNot
{
    Prepared,
    NotPrepared
}

public enum PooledOrNot
{
    Pooled,
    Unpooled
}
}
