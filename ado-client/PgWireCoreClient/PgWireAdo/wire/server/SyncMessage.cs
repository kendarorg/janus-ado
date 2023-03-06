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
        

        public override void Write(PgwByteBuffer stream)
        {
            ConsoleOut.WriteLine("[SERVER] Read: SyncMessage");
            stream.WriteByte((byte)'S');
            stream.WriteInt32(4);
        }
    }
}
