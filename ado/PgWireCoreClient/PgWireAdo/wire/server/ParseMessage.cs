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


    public override void Write(PgwByteBuffer stream)
    {
        ConsoleOut.WriteLine("[SERVER] Write: ParseMessage " + _query);
        if (_query == null) throw new InvalidOperationException("Missing query");
        int length =  4 + _query.Length + 1 + _preparedStatementName.Length + 1+2+ _oids.Count*4;
        stream.WriteByte((byte)'P');
        stream.WriteInt32(length);
        stream.WriteASCIIString(_preparedStatementName);
        stream.WriteByte(0);
        stream.WriteASCIIString(_query);
        stream.WriteByte(0);
        stream.WriteInt16((short)_oids.Count);
        foreach (var oid in _oids)
        {
            stream.WriteInt32(length);
        }
    }

    
}