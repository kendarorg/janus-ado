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
        public override bool IsMatching(ReadSeekableStream stream)
        {
            return ReadData(stream, () =>
                {
                    var res = (byte)stream.ReadByte();
                    return res == (byte)BackendMessageCode.ParseComplete;
                }
                );
        }

        public override void Read(ReadSeekableStream stream)
        {
            System.Diagnostics.Trace.WriteLine("ParseComplete");
            stream.ReadByte();
            stream.ReadInt32();
        }
    }
}
