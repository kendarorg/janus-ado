package org.example.builders;

import org.example.messages.QueryMessage;
import org.example.messages.commons.ErrorResponse;
import org.example.messages.extendedquery.ParseMessage;
import org.example.messages.querymessage.CommandComplete;
import org.example.messages.querymessage.DataRow;
import org.example.messages.querymessage.Field;
import org.example.messages.querymessage.RowDescription;
import org.example.server.Context;
import org.example.server.TypesOids;

import java.nio.ByteBuffer;
import java.nio.charset.StandardCharsets;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Types;
import java.util.ArrayList;
import java.util.concurrent.Future;

public class RealResultBuilder {
    public static Future<Integer> buildRealResult(QueryMessage queryMessage, Context client) {
        var conn = client.getConnection();
        var query = queryMessage.getQuery();
        try {
            var result = conn.createStatement().execute(query);
            if(!result){
                CommandComplete commandComplete = new CommandComplete(query);
                return client.write(commandComplete);
            }
        } catch (SQLException e) {
            throw new RuntimeException(e);
        }
        return null;
    }

    public static Future<Integer> buildRealResult(ParseMessage parseMessage, Context client) {

        Future<Integer> writeResult;
        var conn = client.getConnection();
        var query = parseMessage.getQuery();
        try {
            var st = conn.createStatement();
            var result = st.execute(query);
            if(result){
                var rs= st.getResultSet();
                var md = rs.getMetaData();

                var fields = new ArrayList<Field>();
                for(var i=0;i<md.getColumnCount();i++){
                    fields.add(new Field(
                            md.getColumnName(i+1),
                            0,
                            0, convertType(md.getColumnType(i+1))
                            , md.getPrecision(i+1), -1, 0));
                }

                RowDescription rowDescription = new RowDescription(fields);
                writeResult = client.write(rowDescription);
                while(rs.next()){
                    var byteRow = new ArrayList<ByteBuffer>();
                    for(var i=0;i<fields.size();i++){
                        byteRow.add(ByteBuffer.wrap(buildData(fields.get(i),rs,i+1)));
                    }
                    DataRow dataRow = new DataRow(byteRow);
                    writeResult = client.write(dataRow);
                }

            }
        } catch (SQLException e) {
            ErrorResponse errorResponse = new ErrorResponse(e.getMessage());
            writeResult= client.write(errorResponse);
        }
        CommandComplete commandComplete = new CommandComplete(query);
        return client.write(commandComplete);
    }

    private static byte[] buildData(Field field, ResultSet rs, int i) throws SQLException {
        var dt = field.getDataTypeObjectId();
        if(dt==TypesOids.Bytea||dt==TypesOids.Varbit){
            return rs.getBytes(i);
        }
        return rs.getString(i).getBytes(StandardCharsets.UTF_8);
    }

    private static int convertType(int columnType) throws SQLException {
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
        }
        throw new SQLException("NOT RECOGNIZED COLUMN TYPE "+columnType);
    }
}
