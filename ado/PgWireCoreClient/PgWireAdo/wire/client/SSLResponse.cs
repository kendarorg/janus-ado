using PgWireAdo.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PgWireAdo.wire.client
{
    public class SSLResponse :PgwClientMessage
    {

        public override void Read(DataMessage stream)
        {
            ConsoleOut.WriteLine("SSLResponse");
        }

        public override BackendMessageCode BeType => BackendMessageCode.NoticeResponse;
    }
}
