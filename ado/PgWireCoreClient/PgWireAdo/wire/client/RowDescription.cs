using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PgWireAdo.utils;

namespace PgWireAdo.wire.client
{
    public class RowDescription : PgwClientMessage
    {
        private List<RowDescriptor> _fields = new ();

        public List<RowDescriptor> Fields => _fields;


        public override bool IsMatching(ReadSeekableStream stream)
        {
            return ReadData(stream, () =>
                stream.ReadByte() == (byte)BackendMessageCode.RowDescription);
        }

        public override void Read(ReadSeekableStream stream)
        {
            stream.ReadByte();
            var length = stream.ReadInt32();
            var closCount = stream.ReadInt16();
            for (var i = 0; i < closCount; i++)
            {
                var name = stream.ReadUTF8String();
                var tableObjectId = stream.ReadInt32();
                var columnAttributeNumber = stream.ReadInt16();
                var dataTypeObjectId = stream.ReadInt32();
                var dataTypeSize = stream.ReadInt16();
                var typeModifier = stream.ReadInt32();
                var formatCode = stream.ReadInt16();
                _fields.Add(new RowDescriptor(name,tableObjectId,
                    columnAttributeNumber,dataTypeObjectId,dataTypeSize,
                    typeModifier,formatCode));
            }
        }
    }
}
