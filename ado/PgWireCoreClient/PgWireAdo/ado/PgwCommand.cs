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

public class PgwCommand : DbCommand,IDisposable
{
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            DbTransaction?.Dispose();
        }

        //base.Dispose(disposing);
    }

    private List<RowDescriptor> _fields;
    private string _statementId;
    private string _portalId;

    public PgwCommand(DbConnection dbConnection)
    {
        DbParameterCollection = new PgwParameterCollection();
        DbConnection = dbConnection;
        CommandText = String.Empty;
        CommandType = CommandType.Text;
    }

    public PgwCommand()
    {
        DbParameterCollection = new PgwParameterCollection();
        CommandText = String.Empty;
        CommandType = CommandType.Text;
    }

    public PgwCommand(string commandText, DbConnection conn = null, DbTransaction dbTransaction = null)
    {
        DbParameterCollection = new PgwParameterCollection();
        CommandType = CommandType.Text;
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
    protected DbParameterCollection Parameters => DbParameterCollection;

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
       var commandComplete = stream.WaitFor<CommandComplete>();
        while (commandComplete!=null)
        {
            result += commandComplete.Count;
            commandComplete = stream.WaitFor<CommandComplete>();
        }
        stream.Write(new SyncMessage());
        var readyForQuery = stream.WaitFor<ReadyForQuery>();
        

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
        stream.Write(new SyncMessage());
        object result = null;
        var dataRow = stream.WaitFor<PgwDataRow>((a) =>
        {
            a.Descriptors = _fields;
        });
        var hasData = false;
        if (dataRow != null)
        {
            if (dataRow.Data.Count > 0)
            {
                var field = _fields[0];
                result = PgwConverter.convert(field, dataRow.Data[0]);
            }
        }
        
        var commandComplete = stream.WaitFor <CommandComplete>();
        while (commandComplete!=null)
        {
            if (!hasData)
            {
                if (result == null) result = 0;
                var tmp = (int)result;
                tmp +=commandComplete.Count;
                result = tmp;
            }
            else
            {
                break;
            }
            commandComplete = stream.WaitFor<CommandComplete>();
            //
        }

        stream.Write(new SyncMessage());
        var readyForQuery = stream.WaitFor < ReadyForQuery>();
        
        return result;
    }

    private void CallQuery()
    {
        var stream = ((PgwConnection)DbConnection).Stream;
        var errorMessage = new ErrorResponse();
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

        var query = CommandText;
        if (CommandType == CommandType.TableDirect)
        {
            query = "SELECT * FROM " + CommandText;
        }

        if (query == null || query.Length == 0)
        {
            throw new InvalidOperationException("Missing query");
        }
        _statementId = Guid.NewGuid().ToString();
        stream.Write(new ParseMessage(_statementId, query, this.Parameters));
        var parseComplete = stream.WaitFor<ParseComplete>();
        
        _portalId = Guid.NewGuid().ToString();
        stream.Write(new BindMessage(_statementId, _portalId, DbParameterCollection));
        var bindComplete = stream.WaitFor < BindComplete>();

        stream.Write(new DescribeMessage('P', _portalId));

        stream.Write(new ExecuteMessage(_portalId, 0));

        var commandReceived = stream.WaitFor<CommandComplete,RowDescription>();
        if (commandReceived is CommandComplete)
        {
            stream.Write(new SyncMessage());
        }
        else
        {
            _fields = ((RowDescription)commandReceived).Fields;
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
