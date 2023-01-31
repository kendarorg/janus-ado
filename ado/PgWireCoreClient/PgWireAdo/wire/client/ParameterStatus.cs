using PgWireAdo.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PgWireAdo.wire.client
{
    public class ParameterStatus:PgwClientMessage
    {
        public override bool IsMatching(ReadSeekableStream stream)
        {
            return ReadData(stream, () =>
                stream.ReadByte() == (byte)BackendMessageCode.ParameterStatus);
        }

        public override void Read(ReadSeekableStream stream)
        {
            stream.ReadByte();
            var length = stream.ReadInt32();
            Key = stream.ReadAsciiString();
            Value = stream.ReadAsciiString();
        }

        public string Key { get; private set; }
        public string Value { get; private set; }
    }
}
