using System.Data.Common;
using PgWireAdo.utils;

namespace PgWireAdo.wire.server;

public class ParseMessage : PgwServerMessage
{
    private String _preparedStatementName;
    private String _query;
    private List<int> _oids = new();

    public ParseMessage(string statementId, string commandText, DbParameterCollection parameters)
    {
        _query = commandText;
        _preparedStatementName = statementId;
        foreach (DbParameter dbParameter in parameters)
        {
            _oids.Add((int)PgwConverter.convert(dbParameter.DbType,dbParameter.Value));
        }
    }


    public override void Write(ReadSeekableStream stream)
    {
        System.Diagnostics.Trace.WriteLine("QueryMessage " + _query);
        if (_query == null) throw new InvalidOperationException("Missing query");
        int length = 1 + 4 + _query.Length + 1 + _preparedStatementName.Length + 1+ _oids.Count*4;
        WriteByte((byte)'P');
        WriteInt32(length);
        WriteASCIIString(_preparedStatementName);
        WriteByte(0);
        WriteASCIIString(_query);
        WriteByte(0);
        WriteInt16(_oids.Count);
        foreach (var oid in _oids)
        {
            WriteInt32(length);
        }
        Flush(stream);
    }

    
}