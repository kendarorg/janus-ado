using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PgWireAdo.utils;
using PgWireAdo.wire.client;
using PgWireAdo.wire.server;

namespace PgWireAdo.ado;

public class PgwCommand : DbCommand
{
    private List<RowDescriptor> _fields;

    public PgwCommand(DbConnection dbConnection)
    {
        DbConnection = dbConnection;
    }

    public PgwCommand()
    {
    }

    public PgwCommand(string commandText, DbConnection conn, DbTransaction dbTransaction = null)
    {
        CommandText = commandText;
        DbConnection = conn;
        DbTransaction = dbTransaction;
    }

    public override string CommandText { get; set; }
    public override int CommandTimeout { get; set; }
    public override CommandType CommandType { get; set; }
    public override UpdateRowSource UpdatedRowSource { get; set; }
    protected override DbConnection? DbConnection { get; set; }
    protected override DbParameterCollection DbParameterCollection { get; }
    protected override DbTransaction? DbTransaction { get; set; }
    public override bool DesignTimeVisible { get; set; }


    protected override DbParameter CreateDbParameter()
    {
        return new PgwParameter();
    }

    public override int ExecuteNonQuery()
    {
        var stream = ((PgwConnection)DbConnection).Stream;
        CallQuery();
        var result = 0;
        var commandComplete = new CommandComplete();
        if (commandComplete.IsMatching(stream))
        {
            commandComplete.Read(stream);
            result = commandComplete.Count;
        }
        var readyForQuery = new ReadyForQuery();
        if (readyForQuery.IsMatching(stream))
        {
            readyForQuery.Read(stream);
        }



        return result;
    }

    public override Task<int> ExecuteNonQueryAsync(CancellationToken cancellationToken)
    {
        return Task.Run(ExecuteNonQuery);
    }

    public override object? ExecuteScalar()
    {
        var stream = ((PgwConnection)DbConnection).Stream;
        CallQuery();
        object result = null;
        var dataRow = new PgwDataRow(_fields);
        if (dataRow.IsMatching(stream))
        {
            dataRow.Read(stream);
            if (dataRow.Data.Count > 0)
            {
                var field = _fields[0];
                result = PgwConverter.convert(field,dataRow.Data[0]);
            }
        }
        var commandComplete = new CommandComplete();
        if (commandComplete.IsMatching(stream))
        {
            commandComplete.Read(stream);
            result = commandComplete.Count;
        }
        var readyForQuery = new ReadyForQuery();
        if (readyForQuery.IsMatching(stream))
        {
            readyForQuery.Read(stream);
        }
        return result;
    }

    public override async Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken)
    {
        return await Task.Run(ExecuteScalar);
    }

    public override void Prepare()
    {
        
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        CallQuery();
        return new PgwDataReader(DbConnection, CommandText, _fields);
    }


    private void CallQuery()
    {
        var stream = ((PgwConnection)DbConnection).Stream;
        if (DbParameterCollection == null || DbParameterCollection.Count == 0)
        {
            var queryMessage = new QueryMessage(CommandText);
            queryMessage.Write(stream);
        }
        else
        {
            throw new NotImplementedException();
        }

        var rowDescription = new RowDescription();
        var errorMessage = new ErrorResponse();
        if (rowDescription.IsMatching(stream))
        {
            rowDescription.Read(stream);
            _fields = rowDescription.Fields;
        }
        if (errorMessage.IsMatching(stream))
        {
            errorMessage.Read(stream);
        }

    }
    public override void Cancel()
    {
        throw new NotImplementedException();
    }

    public new Task<DbDataReader> ExecuteReaderAsync()
    {
        return ExecuteReaderAsync(CommandBehavior.Default);
    }

    public new Task<DbDataReader> ExecuteReaderAsync(CommandBehavior commandBehavior)
    {
        return Task.Run(() => ExecuteDbDataReader(commandBehavior));
    }
}
