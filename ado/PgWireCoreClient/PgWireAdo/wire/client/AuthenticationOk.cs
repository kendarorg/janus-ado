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
        public override bool IsMatching(ReadSeekableStream stream)
        {
            return ReadData(stream, () =>
                stream.ReadByte() == (byte)BackendMessageCode.AuthenticationRequest
                && stream.ReadInt32() == 8
                && stream.ReadInt32() == 0);
        }

        public override void Read(ReadSeekableStream stream)
        {
            System.Diagnostics.Trace.WriteLine("AutenthicationOk");
            stream.ReadByte();
            stream.ReadInt32();
            stream.ReadInt32();
        }
    }
}
