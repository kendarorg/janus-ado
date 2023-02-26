using System.ComponentModel;
using System.Data;
using PgWireAdo.wire.client;

namespace PgWireAdo.utils
{
    public class PgwConverter
    {
        public static TypesOids convert(DbType? type, object? value)
        {
            if (type.HasValue)
            {
                switch (type)
                {
                    case (DbType.AnsiString): return TypesOids.Varchar;
                    case (DbType.String): return TypesOids.Varchar;
                    case (DbType.Boolean): return TypesOids.Bit;
                    case (DbType.Byte): return TypesOids.Char;
                    case (DbType.Int16): return TypesOids.Int2;
                    case (DbType.Int32): return TypesOids.Int4;
                    case (DbType.Int64): return TypesOids.Int8;
                    default:
                        throw new Exception();
                }
            }else if (value != null)
            {
                if(value.GetType()==typeof(String))return TypesOids.Varchar;
                if (value.GetType() == typeof(int)) return TypesOids.Int4;
                if (value.GetType() == typeof(long)) return TypesOids.Int8;
                throw new InvalidOperationException("Missing type");
            }
            else
            {
                throw new InvalidOperationException("Missing type");
            }

        }
        public static object? convert(RowDescriptor field, object o)
        {
            if (field.FormatCode == 0)
            {
                if (o == null)
                {
                    return DBNull.Value;
                }
                var s = (String)o;
                var doid = (TypesOids)field.DataTypeObjectId;
                switch (doid)
                {
                    case (TypesOids.Void): 
                    return null;
                    case (TypesOids.Int2): return short.Parse(s);
                    case (TypesOids.Int4): return int.Parse(s);
                    case (TypesOids.Int8): return long.Parse(s);
                    case (TypesOids.Bool): return bool.Parse(s);
                    case (TypesOids.Varchar):
                    case (TypesOids.Xml):
                    case (TypesOids.Text):
                    case (TypesOids.Json): return s;
                    default:
                        throw new Exception();
                }
                //from string
            }
            else
            {
                throw new Exception();
                //from bytes
            }
        }
    }
}
