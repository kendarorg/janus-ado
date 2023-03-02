using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PgWireAdo.utils;

namespace PgWireAdo.wire.client
{
    public class ParseComplete:PgwClientMessage
    {

        public override BackendMessageCode BeType => BackendMessageCode.ParseComplete;
        public override void Read(DataMessage stream)
        {
            ConsoleOut.WriteLine("[SERVER] Read: ParseComplete");
        }
    }
}
