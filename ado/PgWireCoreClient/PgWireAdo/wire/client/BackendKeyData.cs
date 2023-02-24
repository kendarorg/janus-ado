using PgWireAdo.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PgWireAdo.wire.client
{
    public class BackendKeyData:PgwClientMessage
    {

        public override bool IsMatching(ReadSeekableStream stream)
        {
            return ReadData(stream, () =>
                stream.ReadByte() == (byte)BackendMessageCode.BackendKeyData
                && stream.ReadInt32() == 12);
        }

        public override void Read(ReadSeekableStream stream)
        {
            System.Diagnostics.Trace.WriteLine("BackendKeyData");
            stream.ReadByte();
            stream.ReadInt32();
            ProcessKey = stream.ReadInt32();
            SecretKey = stream.ReadInt32();
        }

        public int SecretKey { get; set; }

        public int ProcessKey { get; set; }
    }
}
