using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PgWireAdo.utils;
using PgWireAdo.utils.parse;
using PgWireAdo.wire.client;
using PgWireAdo.wire.server;

namespace PgWireAdo.ado;

public class PgwCommand : DbCommand,IDisposable
{
    protected override void Dispose(bool disposing)
    {
        _disposed =true;
        if (disposing)
        {
            DbTransaction?.Dispose();
        }

        //base.Dispose(disposing);
    }

    private List<RowDescriptor> _fields;
    private string _statementId;
    private string _portalId;
    private int _lastExecuteRequest;

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
         if (_queries[_currentQuery].Type == SqlStringType.CALL || _queries[_currentQuery].Type == SqlStringType.SELECT)
         {
             var dataRow = stream.WaitFor<PgwDataRow>((a) => { a.Descriptors = _fields; });
             while (dataRow != null)
             {
                 dataRow = stream.WaitFor<PgwDataRow>((a) => { a.Descriptors = _fields; });
             }
         }

         var result = 0;

       var commandComplete = stream.WaitFor<CommandComplete>();
        while (commandComplete!=null)
        {
            result += commandComplete.Count;
            commandComplete = stream.WaitFor<CommandComplete>(timeout:10L);
        }
        stream.Write(new SyncMessage());
        var readyForQuery = stream.WaitFor<ReadyForQuery>();
        

        return result;
    }

    
    

    public override void Prepare()
    {
        if (_disposed) throw new ObjectDisposedException("DbCommand");

    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        CallQuery();
        var result = new PgwDataReader(DbConnection, this, _fields,behavior,this._lastExecuteRequest);
        result.PreLoadData();
        
        return result;
    }



    public override object? ExecuteScalar()
    {
        if (DbConnection == null) throw new InvalidOperationException("Missing connection");
        
        CallQuery();
        var stream = ((PgwConnection)DbConnection).Stream;
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
                hasData = true;
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
        var readyForQuery = stream.WaitFor <ReadyForQuery>();
        
        return result;
    }

    private List<SqlParseResult> _queries;
    private int _currentQuery = 0;
    private string _commandText;
    private bool _disposed=false;

    public override string CommandText
    {
        get => _commandText;
        set
        {
            _commandText = value;
            _queries = SetupQueries(SqlParser.getTypes(value),value);
        }
    }

    public SqlParseResult CurrentQuery => _queries[_currentQuery];

    public List<RowDescriptor> Fields => _fields;

    private List<SqlParseResult> SetupQueries(List<SqlParseResult> queries,String query)
    {
        if(SqlParser.isUnknown(queries)){
            return new 
            List<SqlParseResult>(){new SqlParseResult(query,SqlStringType.UNKNOWN)};

        }
        return queries;
    }


    public bool HasNextResult()
    {
        return _queries[_currentQuery].Type==SqlStringType.SELECT;
    }
    public bool NextResult()
    {
        if (_queries.Count > (_currentQuery + 1))
        {
            _currentQuery++;
            return true;
        }
        return false;
    }

    public void CallQuery()
    {

        if (_disposed) throw new ObjectDisposedException("DbCommand");
        var stream = ((PgwConnection)DbConnection).Stream;
        if (stream == null) throw new InvalidOperationException();
        if (CommandType == CommandType.TableDirect)
        {
            _queries = new List<SqlParseResult>
            {
                new ("SELECT * FROM " + CommandText + ";", SqlStringType.SELECT)
            };
        }

        if (_queries == null || _queries.Count == 0 || _currentQuery >= _queries.Count)
        {
            throw new InvalidOperationException();
        }
        var query = _queries[_currentQuery].Value;
        

        if (query == null || query.Length == 0)
        {
            throw new InvalidOperationException("Missing query");
        }
        _statementId = Guid.NewGuid().ToString();

        SqlParameterType parametersType;
        var parametersCollection =(PgwParameterCollection) DbParameterCollection;
        var parameters = SqlParser.getParameters(query, out parametersType);
  
        var parsedNamed = parameters.
            FindAll(t=>t.Named).
            GroupBy(test => test.Name).
            Select(grp => grp.First()).
            Select(i=>
            {
                if(i.Name.StartsWith(":")|| i.Name.StartsWith("@")) return i.Name.Substring(1);
                return i.Name;
            }).ToList();
        for (var index =(parametersCollection.Data.Count-1); index >=0 ; index--)
        {
            var parsed = parametersCollection.Data[index];
            if (parsed.ParameterName == null) continue;
            var namOpe = parsed.ParameterName;
            if(namOpe.StartsWith(":")|| namOpe.StartsWith("@")) namOpe = namOpe.Substring(1);
            var founded = parsedNamed.FindIndex(a => a == namOpe);
            if (founded <0)
            {
                parametersCollection.Data.RemoveAt(index);
            }
        }


        if (parametersType==SqlParameterType.NAMED)
        {
            parametersCollection = (PgwParameterCollection)SqlParser.MaskParameters(ref query, parameters, DbParameterCollection, parametersType);
        }
        stream.Write(new ParseMessage(_statementId, query, parametersCollection));
        var parseComplete = stream.WaitFor<ParseComplete>();
        
        _portalId = Guid.NewGuid().ToString();
        stream.Write(new BindMessage(_statementId, _portalId, parametersCollection));
        var bindComplete = stream.WaitFor <BindComplete>();

        stream.Write(new DescribeMessage('P', _portalId));

        var rowDescription = stream.WaitFor<RowDescription>();
        _fields = rowDescription.Fields;
        

        _lastExecuteRequest = 0;
        stream.Write(new ExecuteMessage(_portalId, 0));



    }

    public override void Cancel()
    {
        throw new NotImplementedException();
    }

    
}
