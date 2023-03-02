using PgWireAdo.utils;
using PgWireAdo.wire.client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PgWireAdo.wire.server
{
    public class SSLNegotation : PgwServerMessage
    {
        public override void Write(PgwByteBuffer stream)
        {
            ConsoleOut.WriteLine("[SERVER] Write: SSLNegotation");
            stream.WriteByte(0x00);
            stream.WriteByte(0x00);
            stream.WriteByte(0x00);
            stream.WriteByte(0x08);

            stream.WriteByte(0x04);
            stream.WriteByte(0xd2);
            stream.WriteByte(0x16);
            stream.WriteByte(0x2f);
        }
    }
}
