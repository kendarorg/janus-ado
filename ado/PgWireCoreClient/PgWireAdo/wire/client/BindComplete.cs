using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PgWireAdo.utils;

namespace PgWireAdo.wire.client
{
    public class BindComplete:PgwClientMessage
    {
        public override bool IsMatching(ReadSeekableStream stream)
        {
            return ReadData(stream, () =>
                stream.ReadByte() == (byte)BackendMessageCode.BindComplete);
        }

        public override void Read(ReadSeekableStream stream)
        {
            ConsoleOut.WriteLine("BindComplete");
            stream.ReadByte();
            stream.ReadInt32();
        }

        public int Count { get; set; }

        public string Tag { get; set; }
    }
}
