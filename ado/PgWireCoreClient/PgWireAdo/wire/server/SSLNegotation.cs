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
        public override void Write(ReadSeekableStream stream)
        {
            ConsoleOut.WriteLine("SSLNegotation");
            WriteByte(0x00);
            WriteByte(0x00);
            WriteByte(0x00);
            WriteByte(0x08);

            WriteByte(0x04);
            WriteByte(0xd2);
            WriteByte(0x16);
            WriteByte(0x2f);
            Flush(stream);
            //DO STUFF
            var sslResponse = new SSLResponse();
            if (sslResponse.IsMatching(stream))
            {
                sslResponse.Read(stream);
                return;
            }
            throw new Exception("[ERROR] SSLResponse");
        }
    }
}
