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
        public override void Write(ReadSeekableStream stream)
        {
            System.Diagnostics.Trace.WriteLine("QueryMessage "+_portal);
            if (_portal == null) throw new InvalidOperationException("Missing query");
            int length = 1 + 4 + _portal.Length+1+4;
            WriteByte((byte)'E');
            WriteInt32(length);
            WriteASCIIString(_portal);
            WriteByte(0);
            WriteInt32(_maxRows);
            Flush(stream);
        }
    }
}
