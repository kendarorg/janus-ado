﻿using System;
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

public class PgwCommand : DbCommand, IDisposable
{
    protected override void Dispose(bool disposing)
    {
        _fields = new List<RowDescriptor>();
        _currentQuery = -1;
        _lastExecuteRequest = 0;
        _queries = new List<SqlParseResult>();
        _portalId = "";
        _statementId = "";
        DbConnection = null;
        DbTransaction?.Dispose();

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
        var result = 0;
        _currentQuery = 0;
        if (_queries == null || _queries.Count == 0)
        {
            throw new InvalidOperationException();
        }
        foreach (var sqlParseResult in _queries)
        {

            var stream = ((PgwConnection)DbConnection).Stream;
            CallQuery();
            if (_fields != null && _fields.Count > 0)
            {
                var dataRow = stream.WaitFor<PgwDataRow>((d) => d.Descriptors = _fields);
                while (dataRow != null)
                {
                    dataRow = stream.WaitFor<PgwDataRow>((d) => d.Descriptors = _fields,10L);
                }
            }

            var commandComplete = stream.WaitFor<CommandComplete>();
            while (commandComplete != null)
            {
                result += ((CommandComplete)commandComplete).Count;
                commandComplete = stream.WaitFor<CommandComplete>(timeout: 10L);
            }
            stream.Write(new SyncMessage());
            var readyForQuery = stream.WaitFor<ReadyForQuery>();
            _currentQuery++;
        }



        return result;
    }




    public override void Prepare()
    {

    }

    protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
    {
        CallQuery();
        var result = new PgwDataReader(DbConnection, this, _fields, behavior, this._lastExecuteRequest);
        result.PreLoadData();
        return result;
    }



    public override object? ExecuteScalar()
    {
        if (DbConnection == null) throw new InvalidOperationException("Missing connection");
        _currentQuery = 0;
        CallQuery();
        var stream = ((PgwConnection)DbConnection).Stream;
        stream.Write(new SyncMessage());
        object result = null;

        var dataRow = stream.WaitFor<PgwDataRow>((a) => { a.Descriptors = _fields; });
        
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

        var commandComplete = stream.WaitFor<CommandComplete>();
        while (commandComplete != null)
        {
            if (!hasData)
            {
                if (result == null) result = 0;
                var tmp = (int)result;
                tmp += commandComplete.Count;
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
        var readyForQuery = stream.WaitFor<ReadyForQuery>();

        return result;
    }

    private List<SqlParseResult> _queries;
    private int _currentQuery = 0;
    private string _commandText;
    public override string CommandText
    {
        get => _commandText;
        set
        {
            _commandText = value;
            _queries = SetupQueries(StringParser.getTypes(value), value);
        }
    }

    public SqlParseResult CurrentQuery => _queries[_currentQuery];

    public List<RowDescriptor> Fields => _fields;

    private List<SqlParseResult> SetupQueries(List<SqlParseResult>? queries, String query)
    {
        if (queries == null) return new List<SqlParseResult>();
        if (StringParser.isUnknown(queries))
        {
            return new
            List<SqlParseResult>() { new SqlParseResult(query, SqlStringType.UNKNOWN) };

        }
        return queries;
    }


    public bool HasNextResult()
    {
        return _queries[_currentQuery].Type == SqlStringType.SELECT;
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
        _fields = new List<RowDescriptor>();
        var stream = ((PgwConnection)DbConnection).Stream;
        if (stream == null)
        {
            throw new InvalidOperationException();
        }
        if (CommandType == CommandType.TableDirect)
        {
            _queries = new List<SqlParseResult>
            {
                new ("SELECT * FROM " + CommandText + ";", SqlStringType.SELECT)
            };
        }

        if (_queries == null || _queries.Count == 0)
        {
            throw new InvalidOperationException();
        }
        var query = _queries[_currentQuery].Value;


        if (query == null || query.Length == 0)
        {
            throw new InvalidOperationException("Missing query");
        }
        _statementId = Guid.NewGuid().ToString();
        stream.Write(new ParseMessage(_statementId, query, this.Parameters));
        var parseComplete = stream.WaitFor<ParseComplete>();

        _portalId = Guid.NewGuid().ToString();
        stream.Write(new BindMessage(_statementId, _portalId, DbParameterCollection));
        var bindComplete = stream.WaitFor<BindComplete>(timeout:2000L);
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
