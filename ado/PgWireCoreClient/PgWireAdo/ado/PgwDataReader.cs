using System.Collections;
using System.Data;
using System.Data.Common;
using System.IO;
using PgWireAdo.utils;
using PgWireAdo.wire.client;
using PgWireAdo.wire.server;

namespace PgWireAdo.ado;

public class PgwDataReader : DbDataReader
{
    private readonly DbConnection _dbConnection;

    public DbConnection DbConnection => _dbConnection;

    public string CommandText => _commandText;

    public List<RowDescriptor> Fields => _fields;

    private readonly string _commandText;
    private List<RowDescriptor> _fields;
    private CommandBehavior _behavior = CommandBehavior.Default;
    private int _lastExecuteRequest;
    private int _currentRow=-1;
    private List<List<object>> _rows = new ();
    private readonly PgwCommand _command;
    private int? _commandCompleteResult;

    public PgwDataReader(DbConnection dbConnection, PgwCommand command, List<RowDescriptor> fields,
        CommandBehavior behavior , int lastExecuteRequest)
    {
        _dbConnection = dbConnection;
        _commandText = command.CommandText;
        _command = command;
        _fields = fields;
        _behavior = behavior;
        _lastExecuteRequest = lastExecuteRequest;
    }

    public override int FieldCount => _fields.Count;

    public override object this[int ordinal]
    {
        get
        {
            return PgwConverter.convert(_fields[ordinal], _rows[_currentRow][ordinal]);
        }
    }

    public override object this[string name] => this[GetOrdinal(name)];

    public override int RecordsAffected { get; }
    public override bool HasRows
    {
        get { return _rows!=null && _rows.Count>0; }
    }
    public override bool IsClosed { get; }


    public override int Depth { get; }

    public override bool GetBoolean(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override byte GetByte(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override long GetBytes(int ordinal, long dataOffset, byte[]? buffer, int bufferOffset, int length)
    {
        throw new NotImplementedException();
    }

    public override char GetChar(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override long GetChars(int ordinal, long dataOffset, char[]? buffer, int bufferOffset, int length)
    {
        throw new NotImplementedException();
    }

    public override DateTime GetDateTime(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override decimal GetDecimal(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override double GetDouble(int ordinal)
    {
        return double.Parse(GetString(ordinal));
    }

    public override float GetFloat(int ordinal)
    {
        return float.Parse(GetString(ordinal));
    }

    public override Guid GetGuid(int ordinal)
    {
        return Guid.Parse(GetString(ordinal));
    }

    public override short GetInt16(int ordinal)
    {
        return short.Parse(GetString(ordinal));
    }

    public override int GetInt32(int ordinal)
    {
        return int.Parse(GetString(ordinal));
    }

    public override long GetInt64(int ordinal)
    {
        return long.Parse(GetString(ordinal));
    }

    public override string GetName(int ordinal)
    {
        return _fields[ordinal].Name;
    }

    public override int GetOrdinal(string name)
    {
        for (var index = 0; index < _fields.Count; index++)
        {
            var rowDescriptor = _fields[index];
            if (name.ToLowerInvariant() == rowDescriptor.Name.ToLowerInvariant())
            {
                return index;
            }
        }

        return -1;
    }

    public override string GetString(int ordinal)
    {
        try
        {
            return (string)_rows[_currentRow][ordinal];
        }
        catch (NullReferenceException)
        {
            throw new InvalidOperationException();
        }
    }

    public override object GetValue(int ordinal)
    {
        return _rows[_currentRow][ordinal];
    }

    public override int GetValues(object[] values)
    {
        throw new NotImplementedException();
    }

    public override bool IsDBNull(int ordinal)
    {
        return _rows[_currentRow][ordinal] == null;
    }


    /// <summary>
    /// Advances the reader to the next result when reading the results of a batch of statements.
    /// </summary>
    /// <returns></returns>
    public override bool NextResult()
    {
        var result = _command.NextResult();
        var nextIsSelect = _command.HasNextResult();
        if (result)
        {
            var current = _command.CurrentQuery;
            if (current.Type == SqlStringType.SELECT)
            {
                _command.CallQuery();
                _fields = _command.Fields;
                PreLoadData();
            }else if (current.Type == SqlStringType.UPDATE || current.Type == SqlStringType.INSERT)
            {
                var scalarData = _command.ExecuteScalar();
                _currentRow = -1;
                _fields = new List<RowDescriptor>();
                _fields.Add(new RowDescriptor(null,0,0,0,0,0,0));
                _rows = new List<List<object>>();
                var row = new List<Object>();
                row.Add(scalarData);
                _rows.Add(row);
            }

        }


        return nextIsSelect && result;
    }

    public void PreLoadData()
    {

        if (DbConnection.State == ConnectionState.Closed) return;
        if ((_behavior & CommandBehavior.SingleRow) != 0)
        {
            _lastExecuteRequest = 1;
        }
        var stream = ((PgwConnection)DbConnection).Stream;
        var total = _lastExecuteRequest == 0 ? int.MaxValue : _lastExecuteRequest;
        _currentRow = -1;
        _rows = new List<List<object>>();
        for (var i = 0; i < total; i++)
        {
            var clientMessage = stream.WaitFor<PgwDataRow,CommandComplete>((d) => { d.Descriptors = _fields; });

            if (clientMessage is PgwDataRow)
            {
                var dataRow = (PgwDataRow)clientMessage;
                if (dataRow.Data.Count > 0)
                {
                    _rows.Add(dataRow.Data);
                }
            }
            else if (clientMessage is CommandComplete)
            {
                var qcom = (CommandComplete)clientMessage;
                if (_command.CurrentQuery.Type == SqlStringType.UPDATE || _command.CurrentQuery.Type == SqlStringType.INSERT)
                {
                    _currentRow = -1;
                    _fields = new List<RowDescriptor>();
                    _fields.Add(new RowDescriptor(null, 0, 0, (int)TypesOids.Int4, 0, 0, 0));
                    _rows = new List<List<object>>();
                    var row = new List<Object>();
                    row.Add(""+qcom.Count);
                    _rows.Add(row);
                }
                stream.Write(new SyncMessage());
                stream.WaitFor<ReadyForQuery>();

                break;
            }
            else
            {
                break;
            }
        }
    }
    /// <summary>
    /// Advances the reader to the next record in a result set.
    /// </summary>
    /// <returns><b>true</b> if there are more rows; otherwise <b>false</b>.</returns>
    /// <remarks>
    /// The default position of a data reader is before the first record. Therefore, you must call Read to begin accessing data.
    /// </remarks>
    public override bool Read()
    {
        if (DbConnection.State == ConnectionState.Closed) return false;
        if ((_behavior & CommandBehavior.SingleRow) != 0)
        {
            _lastExecuteRequest = 1;
        }
        var stream = ((PgwConnection)DbConnection).Stream;
        
        if (_currentRow < (_rows.Count - 1))
        {
            _currentRow++;
            return true;
        }
        if ((_behavior & CommandBehavior.CloseConnection) != 0)
        {
            DbConnection.Close();
        }
        return false;

    }

    



    public override Type GetFieldType(int ordinal)
    {
        throw new NotImplementedException();
    }


    public override string GetDataTypeName(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override IEnumerator GetEnumerator()
    {

        return new PgwDbEnumerator(this);
    }

    

    

    new T GetFieldValue<T>(int ordinal)
    {
        var value = GetValue(ordinal);
        if (value == null) return default(T);
        if (value.GetType() == typeof(T))
        {
            return (T)value;
        }
        throw new NotImplementedException();
    }

}