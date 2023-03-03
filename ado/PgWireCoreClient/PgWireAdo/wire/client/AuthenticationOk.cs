using PgWireAdo.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PgWireAdo.wire.client
{
    public class AuthenticationOk:PgwClientMessage
    {
        public override BackendMessageCode BeType => BackendMessageCode.AuthenticationRequest;
       

        public override void Read(DataMessage stream)
        {
            ConsoleOut.WriteLine("[SERVER] Read: AutenthicationOk");
            stream.ReadInt32();
        }
    }
}
