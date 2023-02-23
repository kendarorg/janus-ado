using System.Data;
using System.Data.Common;
using System.Xml.Linq;
using PgWireAdo.ado;

namespace PgWireAdo.utils;

public static class PgwExtesnions{
    public static void AddWithValue(this DbParameterCollection coll,String name,Object?value){
        var dbp = new PgwParameter()
        {
            Value = value,
            ParameterName = name
        };
        coll.Add(dbp);
    }

    public static void AddWithValue(this DbParameterCollection coll, Object? value)
    {
        var dbp = new PgwParameter()
        {
            Value = value
        };
        coll.Add(dbp);
    }

    public static void AddWithValue(this DbParameterCollection coll,String name, DbType type, Object? value)
    {
        var dbp = new PgwParameter(name, type)
        {
            Value = value
        };
        coll.Add(dbp);
    }

    public static async Task<int> ExecuteNonQueryAsync(
        this DbConnection conn, string sql, DbTransaction? tx = null, CancellationToken cancellationToken = default)
    {
        await using var command = tx == null ? new PgwCommand(sql, conn) : new PgwCommand(sql, conn, tx);
        return await command.ExecuteNonQueryAsync(cancellationToken);
    }

    public static int ExecuteNonQuery(
        this DbConnection conn, string sql, DbTransaction? tx = null)
    {
         using var command = tx == null ? new PgwCommand(sql, conn) : new PgwCommand(sql, conn, tx);
        return command.ExecuteNonQuery();
    }
    public static async Task<object?> ExecuteScalar(
        this DbConnection conn, string sql, DbTransaction? tx = null)
    {
        using var command = tx == null ? new PgwCommand(sql, conn) : new PgwCommand(sql, conn, tx);
        return command.ExecuteScalar();
    }
    public static async Task<object?> ExecuteScalarAsync(
        this DbConnection conn, string sql, DbTransaction? tx = null, CancellationToken cancellationToken = default)
    {
        await using var command = tx == null ? new PgwCommand(sql, conn) : new PgwCommand(sql, conn, tx);
        return await command.ExecuteScalarAsync(cancellationToken);
    }
}