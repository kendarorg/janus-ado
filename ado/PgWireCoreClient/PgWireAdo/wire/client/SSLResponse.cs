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
        public bool IsMatching(ReadSeekableStream stream)
        {
            var position = stream.Position;
            var result = stream.ReadByte() == (byte)BackendMessageCode.NoticeResponse;
            stream.Position=position;
            return result;
        }

        public void Read(ReadSeekableStream stream)
        {
            stream.ReadByte();
        }
    }
}
