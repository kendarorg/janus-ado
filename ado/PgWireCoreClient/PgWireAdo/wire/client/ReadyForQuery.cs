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
        public override bool IsMatching(ReadSeekableStream stream)
        {
            return ReadData(stream, () =>
                stream.ReadByte() == (byte)BackendMessageCode.ReadyForQuery);
        }

        public override void Read(ReadSeekableStream stream)
        {
            stream.ReadByte();
            stream.ReadInt32();
            stream.ReadByte();
        }
    }
}
