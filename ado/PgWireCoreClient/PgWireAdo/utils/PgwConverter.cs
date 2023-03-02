using System.Buffers.Binary;
using System.ComponentModel;
using System.Data;
using PgWireAdo.ado;
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
                    case (DbType.Binary): return TypesOids.Bytea;
                    default:
                        throw new Exception();
                }
            }
            else if (value != null)
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

        private static Dictionary<Type, DbType> typeMap;

        static PgwConverter()
        {
            typeMap= new Dictionary<Type, DbType>();
            typeMap[typeof(string)] = DbType.String;
            typeMap[typeof(char[])] = DbType.Binary;
            typeMap[typeof(int)] = DbType.Int32;
            typeMap[typeof(Int32)] = DbType.Int32;
            typeMap[typeof(Int16)] = DbType.Int16;
            typeMap[typeof(Int64)] = DbType.Int64;
            typeMap[typeof(Byte[])] = DbType.Binary;
            typeMap[typeof(Boolean)] = DbType.Boolean;
            typeMap[typeof(DateTime)] = DbType.DateTime2;
            typeMap[typeof(DateTimeOffset)] = DbType.DateTimeOffset;
            typeMap[typeof(Decimal)] = DbType.Decimal;
            typeMap[typeof(Double)] = DbType.Double;
            typeMap[typeof(Decimal)] = DbType.Decimal;
            typeMap[typeof(Byte)] = DbType.Byte;
            typeMap[typeof(TimeSpan)] = DbType.Time;
        }
        public static DbType ConvertToDbType(Object? val)
        {
            if (!typeMap.ContainsKey(val.GetType()))
            {
                throw new InvalidOperationException("Invalid conversion for "+val.GetType());
            }

            return typeMap[val.GetType()];
        }
        public static TypesOids getOidType(object? value)
        {
            if (value == null) return TypesOids.Void;
            switch (value.GetType().Name.ToLowerInvariant())
            {
                case "string": return TypesOids.Varchar;
                case "bool":
                case "boolean": return TypesOids.Bit;
                case "byte": return TypesOids.Char;
                case "short": return TypesOids.Int2;
                case "int": return TypesOids.Int4;
                case "long": return TypesOids.Int8;
                default:
                    throw new Exception();
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

        public static byte[] toBytes(object v)
        {
            if (v.GetType() == typeof(byte[]))
            {
                return (byte[])v;
            }
            else if(v.GetType() == typeof(long))
            {
                return BitConverter.GetBytes(BinaryPrimitives.ReverseEndianness((long)v));
            }
            else if (v.GetType() == typeof(int))
            {
                return BitConverter.GetBytes(BinaryPrimitives.ReverseEndianness((int)v));
            }
            else if (v.GetType() == typeof(Int16))
            {
                return BitConverter.GetBytes(BinaryPrimitives.ReverseEndianness((short)v));
            }
            else if (v.GetType() == typeof(Char[]))
            {
                var cha = (char[])v;
                var ba = new byte[cha.Length];
                for (var index = 0; index < cha.Length; index++)
                {
                    ba[index] = (byte)cha[index];
                }

                return ba;
            }
            else if (v.GetType() == typeof(byte[]))
            {
                return (byte[])v;
            }

            throw new NotImplementedException(v.GetType().Name);
        }
    }
}
