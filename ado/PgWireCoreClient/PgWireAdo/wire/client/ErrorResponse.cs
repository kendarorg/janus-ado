using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PgWireAdo.utils;

namespace PgWireAdo.wire.client
{
    public class ErrorResponse: PgwClientMessage
    {

        public override BackendMessageCode BeType => BackendMessageCode.ErrorResponse;
        public override void Read(DataMessage stream)
        {
            ConsoleOut.WriteLine("[SERVER] Read: ErrorResponse");
            var severity = (char)stream.ReadByte();
            var level = stream.ReadUTF8String();
            var type = (char)stream.ReadByte();
            var message = stream.ReadUTF8String();
            ConsoleOut.WriteLine("[SERVER]          "+level+" "+message);
            throw new Exception("["+level+"]"+message);
        }
    }
}
