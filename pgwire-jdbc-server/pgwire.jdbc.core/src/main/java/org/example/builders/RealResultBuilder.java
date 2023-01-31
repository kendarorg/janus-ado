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
import java.sql.*;
import java.util.ArrayList;
import java.util.concurrent.Future;

public class RealResultBuilder {
    public static Future<Integer> buildRealResult(QueryMessage queryMessage, Context client) {
        var conn = client.getConnection();
        var query = queryMessage.getQuery();
        try {
            var st=conn.createStatement();
            var result = st.execute(query);
            if(!result){
                var updateCount = st.getUpdateCount();
                CommandComplete commandComplete = new CommandComplete("RESULT "+updateCount);
                return client.write(commandComplete);
            }else{
                loadResultset(client, st);
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
            if(query.startsWith(":JANUS:")){
                return handleSpecialQuery(conn,query,client);
            }

            var result = false;
            Statement st = null;
            if(parseMessage.getBinds()!=null && parseMessage.getBinds().size()>0){
                var bind = parseMessage.getBinds().get(0);
                if(bind.getParameterValues().size()>0){
                    st = conn.prepareStatement(query);
                    for(var i=0;i<bind.getParameterValues().size();i++){
                        if(bind.getParamFormatCodes()[i]==0){
                            ((PreparedStatement)st).setString(i+1,(String)bind.getParameterValues().get(i));
                        }else{
                            ((PreparedStatement)st).setBytes(i+1,(byte[])bind.getParameterValues().get(i));
                        }
                    }
                    result = ((PreparedStatement) st).execute();
                }
            }
            if(st==null) {
                st = conn.createStatement();
                result = st.execute(query);
            }
            if(result){
                loadResultset(client, st);
            }
        } catch (SQLException e) {
            ErrorResponse errorResponse = new ErrorResponse(e.getMessage());
            writeResult= client.write(errorResponse);
        }
        CommandComplete commandComplete = new CommandComplete(query);
        return client.write(commandComplete);
    }

    private static void loadResultset(Context client, Statement st) throws SQLException {
        Future<Integer> writeResult;
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

    private static Future<Integer> handleSpecialQuery(Connection conn, String query, Context client) throws SQLException {
        if (query.equalsIgnoreCase("JANUS:BEGIN_TRANSACTION")) {
            if(conn.getAutoCommit())conn.setAutoCommit(false);
        } else if (query.equalsIgnoreCase("JANUS:ROLLBACK_TRANSACTION")) {
            conn.rollback();
        } else if (query.equalsIgnoreCase("JANUS:COMMIT_TRANSACTION")) {
            conn.commit();
            conn.setAutoCommit(true);
        }else if (query.startsWith("JANUS:SET_SAVEPOINT:")) {
            if(conn.getAutoCommit())conn.setAutoCommit(false);
            var val = query.split(":");
            if(val[2].isEmpty()) {
                var savepoint = conn.setSavepoint();
                client.add(savepoint);
            }else{
                var savepoint = conn.setSavepoint(val[2]);
                client.add(savepoint);
            }
        }else if (query.startsWith("JANUS:RELEASE_SAVEPOINT:")) {
            var val = query.split(":");

            Savepoint svp = getSavepoint(client, val);
            if(svp!=null){
                conn.releaseSavepoint(svp);
            }
        }else if (query.startsWith("JANUS:ROLLBACK_SAVEPOINT:")) {
            var val = query.split(":");

            Savepoint svp = getSavepoint(client, val);
            if(svp!=null){
                conn.rollback(svp);
            }
        }
        throw new RuntimeException("NOT IMPLEMENTED SPECIAL QUERIES");
    }

    private static Savepoint getSavepoint(Context client, String[] val) {
        var svp =(Savepoint) client.get((o)->{
            if(!(o instanceof Savepoint))return false;
            var sp = (Savepoint)o;
            var name = "";
            try{
                name = sp.getSavepointName();
            }catch (Exception ex){}
            return name.equalsIgnoreCase(val[2]);
        });
        return svp;
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
