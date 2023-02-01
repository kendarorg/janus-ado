using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PgWireAdo.utils;

namespace PgWireAdo.wire.client
{
    public class CommandComplete:PgwClientMessage
    {
        public override bool IsMatching(ReadSeekableStream stream)
        {
            return ReadData(stream, () =>
                stream.ReadByte() == (byte)BackendMessageCode.CommandComplete);
        }

        public override void Read(ReadSeekableStream stream)
        {
            stream.ReadByte();
            stream.ReadInt32();
            var data = stream.ReadAsciiString();
            try
            {
                var spl = data.Split(" ");
                Tag = spl[0];
                Count = int.Parse(spl[1]);
            }catch(Exception){}
        }

        public int Count { get; set; }

        public string Tag { get; set; }
    }
}
