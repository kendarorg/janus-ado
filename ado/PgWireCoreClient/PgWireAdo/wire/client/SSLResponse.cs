using PgWireAdo.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PgWireAdo.wire.client
{
    public class SSLResponse :PgwClientMessage
    {
        public override bool IsMatching(ReadSeekableStream stream)
        {
            return ReadData(stream, () =>
            {
                var dd = stream.ReadByte();
                return dd == (byte)BackendMessageCode.NoticeResponse;
            });

        }

        public override void Read(ReadSeekableStream stream)
        {
            ConsoleOut.WriteLine("SSLResponse");
            stream.ReadByte();
        }
    }
}
