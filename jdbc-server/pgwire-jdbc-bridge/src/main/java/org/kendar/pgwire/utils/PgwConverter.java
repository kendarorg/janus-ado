package org.kendar.pgwire.utils;

import org.kendar.util.convert.TypeConverter;

import java.math.BigDecimal;
import java.math.BigInteger;
import java.math.MathContext;
import java.nio.ByteBuffer;
import java.nio.charset.StandardCharsets;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Types;
import java.util.Locale;

public class PgwConverter {
    public static Object toPgWire(int formatCode,Class<?> clazz, Object o, int clPrec, int clScale) {
        if(formatCode==1){
            return toPgWireByte(clazz.getName(),(byte[])o,clPrec,clScale);
        }
        return toPgWireString(clazz,(String)o);
    }
    public static Object toPgWireString(Class<?> clazz, String value) {
        try {
            return TypeConverter.convert(clazz, value);
        }catch (Exception ex){
            throw new RuntimeException(ex);
        }
    }



    public static boolean isByteIn(String clName){
        var ns = clName.split("\\.");
        var name = ns[ns.length-1].toLowerCase(Locale.ROOT);
        switch(name){
            case("[b"):
            case("[c"):
            case("float"):
            case("double"):
            case("int"):
            case("long"):
            case("boolean"):
            case("bool"):
            case("byte"):
            case("char"):
            case("bigdecimal"):
            case("timestamp"):
                return true;
            default:
                return false;
        }
    }
    private static Object toPgWireByte(String clName, byte[] o, int clPrec, int clScale) {
        var ns = clName.split("\\.");
        var name = ns[ns.length-1].toLowerCase(Locale.ROOT);
        switch(name){
            case("[b"):
            case("[c"):
                return o;
            case("float"):
                return ByteBuffer.wrap(o).getFloat();
            case("string"):
                return new String(o);
            case("double"):
                return ByteBuffer.wrap(o).getDouble();
            case("int"):
                return ByteBuffer.wrap(o).getInt();
            case("long"):
                return ByteBuffer.wrap(o).getLong();
            case("boolean"):
            case("bool"):
                return o[0]>0;
            case("byte"):
            case("char"):
                return o[0];
            case("bigdecimal"):
                var intVal = new BigInteger(o);
                return new BigDecimal(intVal, clScale, new MathContext(clPrec));
            case("timestamp"):
                //var intVal = new BigInteger(o);
                //return new BigDecimal(intVal, clScale, new MathContext(clPrec));
                return null;
        }
        return null;
    }

    public static int toPgwType(int columnType) throws SQLException {
        switch(columnType){
            case Types.BIGINT:return TypesOids.Int8;
            case Types.ARRAY:return TypesOids.TsVector;
            case Types.BIT:return TypesOids.Bool;
            case Types.BINARY:return TypesOids.Bytea;
            case Types.BLOB:return TypesOids.Varbit;
            case Types.CHAR:return TypesOids.BPChar;
            case Types.CLOB:return TypesOids.Varchar;
            case Types.DATE:return TypesOids.Date;
            case Types.DECIMAL:return TypesOids.Numeric;
            case Types.DOUBLE:return TypesOids.Float8;
            case Types.INTEGER:return TypesOids.Int4;
            case Types.LONGNVARCHAR:return TypesOids.Varchar;
            case Types.LONGVARBINARY:return TypesOids.Varbit;
            case Types.VARCHAR:return TypesOids.Varchar;
            case Types.VARBINARY:return TypesOids.Varbit;
            case Types.NCHAR:return TypesOids.Varchar;
            case Types.NCLOB:return TypesOids.Varchar;
            case Types.NUMERIC:return TypesOids.Numeric;
            case Types.REAL:return TypesOids.Numeric;
            case Types.SMALLINT:return TypesOids.Int2;
            case Types.TIME:return TypesOids.Time;
            case Types.TIME_WITH_TIMEZONE:return TypesOids.TimeTz;
            case Types.TIMESTAMP:return TypesOids.Timestamp;
            case Types.TIMESTAMP_WITH_TIMEZONE:return TypesOids.TimestampTz;
            case Types.TINYINT:return TypesOids.Int2;
            case Types.SQLXML:return TypesOids.Varchar;
            case Types.ROWID:return TypesOids.Int8;
            case 0:return TypesOids.Void;
        }
        throw new SQLException("NOT RECOGNIZED COLUMN TYPE "+columnType);
    }


    public static boolean isByteOut(String clName){
        var ns = clName.split("\\.");
        var name = ns[ns.length-1].toLowerCase(Locale.ROOT);
        switch(name){
            case("[b"):
            case("[c"):
            case("byte"):
            case("char"):
            case("timestamp"):
                return true;
            default:
                return false;
        }
    }
    public static ByteBuffer toBytes(Field field, ResultSet rs, int i) throws SQLException {
        if(!isByteOut(field.getColumnClassName())){
            if(rs.getString(i)==null){
                return ByteBuffer.wrap(new byte[]{});
            }else {
                return ByteBuffer.wrap(rs.getString(i).getBytes(StandardCharsets.UTF_8));
            }
        }
        var ns = field.getColumnClassName().split("\\.");
        var name = ns[ns.length-1].toLowerCase(Locale.ROOT);

        switch(name){
            case("[b"):
            case("[c"):
                var dt= rs.getBytes(i);
                return ByteBuffer.allocate(dt.length).put(dt);
            case("byte"):
            case("char"):
                return ByteBuffer.allocate(1).put(rs.getByte(i));
            default:
                if(rs.getString(i)==null){
                    return ByteBuffer.allocate(0);
                }
                return ByteBuffer.wrap(rs.getString(i).getBytes(StandardCharsets.UTF_8));
        }
    }
}
