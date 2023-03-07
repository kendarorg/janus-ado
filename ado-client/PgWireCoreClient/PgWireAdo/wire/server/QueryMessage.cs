using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using PgWireAdo.utils;

namespace PgWireAdo.wire.server
{
    public class QueryMessage : PgwServerMessage
    {
        private readonly string _query;

        public QueryMessage(String query)
        {
            _query = query;
        }
        public override void Write(PgwByteBuffer stream)
        {
            ConsoleOut.WriteLine("[SERVER] Read: QueryMessage " + _query);
            if (_query == null) throw new InvalidOperationException("Missing query");
            var query = EncodingUtils.GetUTF8(_query);
            int length = 1 + 4 + query.Length;
            stream.WriteByte((byte)'Q');
            stream.WriteInt32(length);
            stream.Write(query.Data);
            stream.WriteByte(0);
        }
    }
}
