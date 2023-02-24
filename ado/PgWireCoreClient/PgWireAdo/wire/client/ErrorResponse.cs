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
        public override bool IsMatching(ReadSeekableStream stream)
        {
            return ReadData(stream, () =>
                stream.ReadByte() == (byte)BackendMessageCode.ErrorResponse);
        }

        public override void Read(ReadSeekableStream stream)
        {
            System.Diagnostics.Trace.WriteLine("ErrorResponse");
            stream.ReadByte();
            var length = stream.ReadInt32();
            var severity = (char)stream.ReadByte();
            var level = stream.ReadUTF8String();
            var type = (char)stream.ReadByte();
            var message = stream.ReadUTF8String();
            throw new Exception("["+level+"]"+message);
        }
    }
}
