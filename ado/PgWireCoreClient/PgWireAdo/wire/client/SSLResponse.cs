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
            return ReadData(stream ,() => 
                stream.ReadByte() == (byte)BackendMessageCode.NoticeResponse);
        }

        public override void Read(ReadSeekableStream stream)
        {
            System.Diagnostics.Trace.WriteLine("SSLResponse");
            stream.ReadByte();
        }
    }
}
