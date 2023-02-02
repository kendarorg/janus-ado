using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PgWireAdo.ado;

namespace PgWireAdo.Test.Utils
{
    public static class TestUtils
    {

        private static int _tempTableCounter = 0;
        public static async Task<int> ExecuteNonQueryAsync(
            this PgwConnection conn, string sql, DbTransaction? tx = null, CancellationToken cancellationToken = default)
        {
            await using var command = tx == null ? new PgwCommand(sql, conn) : new PgwCommand(sql, conn, tx);
            return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public static async Task<string> CreateTempTable(DbConnection conn, string columns)
        {
            var tableName = "temp_table" + Interlocked.Increment(ref _tempTableCounter);

            await conn.ExecuteNonQueryAsync(@$"
DROP TABLE IF EXISTS {tableName} CASCADE;
CREATE TABLE {tableName} ({columns});");

            return tableName;
        }

        public static async Task<int> ExecuteNonQueryAsync(
            this DbConnection conn, string sql, DbTransaction? tx = null, CancellationToken cancellationToken = default)
        {
            await using var command = tx == null ? new PgwCommand(sql, conn) : new PgwCommand(sql, conn, tx);
            return await command.ExecuteNonQueryAsync(cancellationToken);
        }

        public static async Task<object?> ExecuteScalarAsync(
            this DbConnection conn, string sql, DbTransaction? tx = null, CancellationToken cancellationToken = default)
        {
            await using var command = tx == null ? new PgwCommand(sql, conn) : new PgwCommand(sql, conn, tx);
            return await command.ExecuteScalarAsync(cancellationToken);
        }
    }

}
