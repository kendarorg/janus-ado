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
    private string _statementId;
    private string _portalId;

    public PgwCommand(DbConnection dbConnection)
    {
        DbParameterCollection = new PgwParameterCollection();
        DbConnection = dbConnection;
    }

    public PgwCommand()
    {
        DbParameterCollection = new PgwParameterCollection();
    }

    public PgwCommand(string commandText, DbConnection conn = null, DbTransaction dbTransaction = null)
    {
        DbParameterCollection = new PgwParameterCollection();
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
        while (commandComplete.IsMatching(stream))
        {
            commandComplete.Read(stream);
            result += commandComplete.Count;
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
        return Task.FromResult(ExecuteNonQuery());
    }

    public override async Task<object?> ExecuteScalarAsync(CancellationToken cancellationToken)
    {
        return await Task.FromResult(ExecuteScalar());
    }

    public override void Prepare()
    {
        
    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        CallQuery();
        var result = new PgwDataReader(DbConnection, CommandText, _fields,behavior);
        
        return result;
    }



    public override object? ExecuteScalar()
    {
        if (DbConnection == null) throw new InvalidOperationException("Missing connection");
        
        var stream = ((PgwConnection)DbConnection).Stream;
        CallQuery();
        object result = null;
        var dataRow = new PgwDataRow(_fields);
        var hasData = false;
        if (dataRow.IsMatching(stream))
        {
            hasData = true;
            dataRow.Read(stream);
            if (dataRow.Data.Count > 0)
            {
                var field = _fields[0];
                result = PgwConverter.convert(field, dataRow.Data[0]);
            }
        }
        var commandComplete = new CommandComplete();
        while (commandComplete.IsMatching(stream))
        {
            commandComplete.Read(stream);
            if (!hasData)
            {
                if (result == null) result = 0;
                var tmp = (int)result;
                tmp +=commandComplete.Count;
                result = tmp;
            }
            //
        }
        var readyForQuery = new ReadyForQuery();
        if (readyForQuery.IsMatching(stream))
        {
            readyForQuery.Read(stream);
        }
        return result;
    }

    private void CallQuery()
    {
        var stream = ((PgwConnection)DbConnection).Stream;
        /*if (DbParameterCollection == null || DbParameterCollection.Count == 0)
        {
            if (CommandType == CommandType.TableDirect)
            {
                var queryMessage = new QueryMessage("SELECT * FROM "+CommandText+";");
                queryMessage.Write(stream);
            }
            else
            {
                var queryMessage = new QueryMessage(CommandText);
                queryMessage.Write(stream);
            }
        }
        else*/
        {
            var query = CommandText;
            if (CommandType == CommandType.TableDirect)
            {
                query = "SELECT * FROM " + CommandText;
            }
            _statementId = Guid.NewGuid().ToString();
            var queryMessage =
                new ParseMessage(_statementId, query, this.Parameters);
            queryMessage.Write(stream);
            //TODO READ ParseCompleted
            _portalId = Guid.NewGuid().ToString();
            //TODO WRITE Bind with _portalId && _statementID
            //TODO READ BindCompleted
            //TODO WRITE Describe P _portalId
            //TODO READ RowDescription
            //TODO WRITE Execute _portalId [NUMBER OF ROWS]
            //TODO READ [NUMBER OF ROWS] DataRow
            //TODO WAIT CommandComplete (no more rows) 
            //TODO      PortalSuspend (other rows
            //TODO WRITE Sync
            //TODO READ Ready for query

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
