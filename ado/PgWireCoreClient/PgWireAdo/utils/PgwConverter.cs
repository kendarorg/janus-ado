using PgWireAdo.wire.client;

namespace PgWireAdo.utils
{
    public class PgwConverter
    {
        public static object? convert(RowDescriptor field, object o)
        {
            if (field.FormatCode == 0)
            {
                var s = (String)o;
                var doid = (TypesOids)field.DataTypeObjectId;
                switch (doid)
                {
                    case (TypesOids.Int2): return short.Parse(s);
                    case (TypesOids.Int4): return int.Parse(s);
                    case (TypesOids.Int8): return long.Parse(s);
                    case (TypesOids.Bool): return bool.Parse(s);
                    case (TypesOids.Varchar):
                    case (TypesOids.Xml):
                    case (TypesOids.Text):
                    case (TypesOids.Json): return bool.Parse(s);
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
