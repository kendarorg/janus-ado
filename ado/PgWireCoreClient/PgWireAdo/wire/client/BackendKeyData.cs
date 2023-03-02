using PgWireAdo.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PgWireAdo.wire.client
{
    public class BackendKeyData:PgwClientMessage
    {
        public override BackendMessageCode BeType => BackendMessageCode.BackendKeyData;
        

        public override void Read(DataMessage stream)
        {
            ConsoleOut.WriteLine("[SERVER] Read: BackendKeyData");
            ProcessKey = stream.ReadInt32();
            SecretKey = stream.ReadInt32();
        }

        public int SecretKey { get; set; }

        public int ProcessKey { get; set; }
    }
}
