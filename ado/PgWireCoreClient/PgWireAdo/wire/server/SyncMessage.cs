using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using PgWireAdo.utils;
using PgWireAdo.wire.client;

namespace PgWireAdo.wire.server
{
    public class SyncMessage : PgwServerMessage
    {
        
        public SyncMessage()
        {
           
        }
        

        public override void Write(ReadSeekableStream stream)
        {
            ConsoleOut.WriteLine("SyncMessage");
            WriteByte((byte)'S');
            WriteInt32(4);
            Flush(stream);
        }
    }
}
