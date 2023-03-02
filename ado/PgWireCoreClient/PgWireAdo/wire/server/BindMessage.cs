using System;
using System.Buffers.Binary;
using System.Data;
using System.Data.Common;
using System.Security.Cryptography;
using PgWireAdo.ado;
using PgWireAdo.utils;
using TB.ComponentModel;

namespace PgWireAdo.wire.server;

public class BindMessage : PgwServerMessage
{
    private String _destinationPortal;
    private String _sourcePsName;
    private List<PgwParameter> _parameters = new ();
    private List<PgwParameter> _results = new();

    public BindMessage(string statementId, string commandText, DbParameterCollection parameters)
    {
        _sourcePsName = statementId;
        _destinationPortal = commandText;
        foreach (DbParameter dbParameter in parameters)
        {
            if (dbParameter.Direction == ParameterDirection.Input)
            {
                _parameters.Add((PgwParameter)dbParameter);
            }
        }

    }


    public override void Write(PgwByteBuffer stream)
    {
        ConsoleOut.WriteLine("[SERVER] Write: BindMessage " + _sourcePsName+" portal: "+ _destinationPortal);
        if (_sourcePsName == null) throw new InvalidOperationException("Missing query");

        var parsLengths = 0;
        foreach (var pgwParameter in _parameters)
        {
            if (pgwParameter == null || pgwParameter.Value == null)
            {
                parsLengths += 2+4;
            }
            else if (pgwParameter.Value.GetType() == typeof(string))
            {
                parsLengths += 2 + 4 + ((String)pgwParameter.Value).Length;
            }
            else if (pgwParameter.Value.GetType() == typeof(byte[]))
            {
                parsLengths += 2 + 4 + ((byte[])pgwParameter.Value).Length;
            }
            else
            {
                var sval = pgwParameter.Value.To<String>();
                parsLengths += 2+4 + sval.Length;
            }

        }

        int length = 4 + _sourcePsName.Length + 1 + _destinationPortal.Length + 1 +
                     //2 + _parameters.Count * 2 +
                     2 + 2+ parsLengths +
                     2 + _results.Count * 2;
        stream.WriteByte((byte)'B');
        stream.WriteInt32(length);
        stream.WriteASCIIString(_destinationPortal);
        stream.WriteByte(0);
        stream.WriteASCIIString(_sourcePsName);
        stream.WriteByte(0);
        stream.WriteInt16((short)_parameters.Count);

        foreach (var oid in _parameters)
        {
            if (oid == null || oid.Value == null)
            {
                stream.WriteInt16(0);//Text
            }
            else if (oid.Value.GetType() == typeof(String))
            {
                stream.WriteInt16(0);
            }
            else if (oid.Value.GetType()== typeof(byte[]))
            {
                stream.WriteInt16(1);
            }
            else
            {
                stream.WriteInt16(0);
            }

        }
        stream.WriteInt16((short)_parameters.Count);
        foreach (var pgwParameter in _parameters)
        {
            if (pgwParameter == null || pgwParameter.Value == null)
            {
                stream.WriteInt32(0);
            }
            else if (pgwParameter.Value.GetType() == typeof(byte[]))
            {
                var bval = (byte[])pgwParameter.Value;
                stream.WriteInt32(bval.Length);
                stream.Write(bval);
            }
            else if(pgwParameter.Value.GetType() == typeof(string))
            {
                stream.WriteInt32(((String)pgwParameter.Value).Length);
                stream.WriteASCIIString((String)pgwParameter.Value);
            }
            else
            {
                var sval = pgwParameter.Value.To<String>();
                stream.WriteInt32(sval.Length);
                stream.WriteASCIIString(sval);
            }

        }
        /*stream.WriteInt16((short)_results.Count);
        foreach (var oid in _results)
        {
            if (oid == null || oid.Value == null || oid.Value.GetType() == typeof(string))
            {
                stream.WriteInt16(0);//Text
            }
            else
            {
                stream.WriteInt16(1);
            }

        }*/
    }

    
}