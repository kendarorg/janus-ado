using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using PgWireAdo.utils;

namespace PgWireAdo.wire.server
{
    public class ExecuteMessage : PgwServerMessage
    {
        private readonly string _portal;
        private readonly int _maxRows;

        public ExecuteMessage(String portal,int maxRows)
        {
            _portal = portal;
            _maxRows = maxRows;
        }
        public override void Write(PgwByteBuffer stream)
        {
            ConsoleOut.WriteLine("[SERVER] Read: ExecuteMessage " + _portal);
            if (_portal == null) throw new InvalidOperationException("Missing query");
            int length =  4 + _portal.Length+1+4;
            stream.WriteByte((byte)'E');
            stream.WriteInt32(length);
            stream.WriteASCIIString(_portal);
            stream.WriteByte(0);
            stream.WriteInt32(_maxRows);
        }
    }
}
