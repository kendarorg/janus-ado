using PgWireAdo.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PgWireAdo.wire.client
{
    public class ReadyForQuery:PgwClientMessage
    {
        public override BackendMessageCode BeType => BackendMessageCode.ReadyForQuery;
        
        public override void Read(DataMessage stream)
        {
            ConsoleOut.WriteLine("ReadyForQuery");
            var type = (char)stream.ReadByte();
        }
    }
}
