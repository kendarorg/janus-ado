using PgWireAdo.utils;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace PgWireAdo.wire.server
{
    public abstract class PgwServerMessage
    {
        private MemoryStream toSend  = new MemoryStream();
        abstract public void Write(ReadSeekableStream stream);

        protected void WriteByte(byte value)
        {
            toSend.WriteByte(value);
        }

        protected void WriteInt32(int value)
        {
            var val = BitConverter.GetBytes(BinaryPrimitives.ReverseEndianness(value));
            toSend.Write(val);
        }

        protected void WriteASCIIString(String value)
        {
            var val = Encoding.ASCII.GetBytes(value);
            toSend.Write(val);
        }

        protected void Flush(Stream stream)
        {
            stream.Write(toSend.ToArray());
        }

        protected void Write(byte[] stream)
        {
            toSend.Write(stream);
        }
    }
}
