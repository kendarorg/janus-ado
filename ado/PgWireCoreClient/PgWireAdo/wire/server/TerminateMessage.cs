using PgWireAdo.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PgWireAdo.wire.server
{
    public class TerminateMessage : PgwServerMessage
    {
        public override void Write(PgwByteBuffer stream)
        {
            ConsoleOut.WriteLine("[SERVER] Write: TerminateMessage");
            stream.WriteByte((byte)'X');
            stream.WriteInt32(4);
        }
    }
}
