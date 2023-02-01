﻿using System.Collections;
using System.Data.Common;
using PgWireAdo.wire.client;

namespace PgWireAdo.ado;

public class PgwDataReader :DbDataReader
{
    private readonly DbConnection _dbConnection;

    public DbConnection DbConnection => _dbConnection;

    public string CommandText => _commandText;

    public List<RowDescriptor> Fields => _fields;

    private readonly string _commandText;
    private readonly List<RowDescriptor> _fields;
    private List<object> _currentRow;

    public PgwDataReader(DbConnection dbConnection, string commandText, List<RowDescriptor> fields)
    {
        _dbConnection = dbConnection;
        _commandText = commandText;
        _fields = fields;
    }

    public override int FieldCount => _fields.Count;

    public override object this[int ordinal] => throw new NotImplementedException();

    public override object this[string name] => throw new NotImplementedException();

    public override int RecordsAffected { get; }
    public override bool HasRows
    {
        get { return true; }
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
        return (string)_currentRow[ordinal];
    }

    public override object GetValue(int ordinal)
    {
        throw new NotImplementedException();
    }

    public override int GetValues(object[] values)
    {
        throw new NotImplementedException();
    }

    public override bool IsDBNull(int ordinal)
    {
        return _currentRow[ordinal] == null;
    }


    /// <summary>
    /// Advances the reader to the next result when reading the results of a batch of statements.
    /// </summary>
    /// <returns></returns>
    public override bool NextResult()
    {
        throw new NotImplementedException();
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
        var stream = ((PgwConnection)DbConnection).Stream;
        var dataRow = new PgwDataRow(_fields);
        var commandComplete = new CommandComplete();
        if (dataRow.IsMatching(stream))
        {
            dataRow.Read(stream);
            if (dataRow.Data.Count > 0)
            {
                _currentRow = dataRow.Data;
                return true;
            }
        }
        else if (commandComplete.IsMatching(stream))
        {
            commandComplete.Read(stream);
            return false;
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
}