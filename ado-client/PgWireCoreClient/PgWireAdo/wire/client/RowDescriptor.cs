using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PgWireAdo.wire.client
{
    public class RowDescriptor
    {
        public string Name { get; }
        public int TableObjectId { get; }
        public short ColumnAttributeNumber { get; }
        public int DataTypeObjectId { get; }
        public short DataTypeSize { get; }
        public int TypeModifier { get; }
        public short FormatCode { get; }

        public RowDescriptor(string name, int tableObjectId, short columnAttributeNumber, int dataTypeObjectId, short dataTypeSize, int typeModifier, short formatCode)
        {
            Name = name;
            TableObjectId = tableObjectId;
            ColumnAttributeNumber = columnAttributeNumber;
            DataTypeObjectId = dataTypeObjectId;
            DataTypeSize = dataTypeSize;
            TypeModifier = typeModifier;
            FormatCode = formatCode;
        }
    }
}
