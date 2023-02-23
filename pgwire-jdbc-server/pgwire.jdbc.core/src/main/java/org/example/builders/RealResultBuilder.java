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
import org.kendar.util.convert.TypeConverter;

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
            var st=conn.prepareCall(query);
            var result = st.execute();
            if(!result){
                var updateCount = st.getUpdateCount();
                while(true){
                    var moreResults = st.getMoreResults();
                    var newUpdate = st.getUpdateCount();
                    if(newUpdate==-1 && moreResults==false)break;
                    updateCount+=newUpdate;
                }
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
            if(parseMessage.getBinds()!=null ){
                var bind = parseMessage.getBinds();
                if(bind.getParameterValues().size()>0){
                    st = conn.prepareStatement(query);
                    var pmd = ((PreparedStatement)st).getParameterMetaData();
                    for(var i=0;i<bind.getParameterValues().size();i++){
                        var clName = pmd.getParameterClassName(i+1);
                        var clPrec = pmd.getPrecision(i+1);
                        var clScale = pmd.getScale(i+1);
                        var sqlType = pmd.getParameterType(i+1);
                        Class<?> clReal;
                        try {
                            clReal = Class.forName(clName);
                            ((PreparedStatement)st).setObject(i+1,
                                    PgwConverter.toPgWire(
                                            bind.getParamFormatCodes()[i],
                                            clReal,
                                            bind.getParameterValues().get(i),
                                            clPrec,clScale),
                                    sqlType,clScale);
                        } catch (Exception e) {
                            throw new RuntimeException(e);
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
                    0, PgwConverter.toPgwType(md.getColumnType(i+1))
                    , md.getPrecision(i+1), -1, PgwConverter.isByteOut(md.getColumnClassName(i+1))?1:0
                    ,md.getColumnClassName(i+1),md.getScale(i+1),md.getColumnType(i+1)));
        }

        RowDescription rowDescription = new RowDescription(fields);
        writeResult = client.write(rowDescription);
        while(rs.next()){
            var byteRow = new ArrayList<ByteBuffer>();
            for(var i=0;i<fields.size();i++){
                byteRow.add(buildData(fields.get(i),rs,i+1));
            }
            DataRow dataRow = new DataRow(byteRow,fields);
            writeResult = client.write(dataRow);
        }
    }

    private static ByteBuffer buildData(Field field, ResultSet rs, int i) throws SQLException {
        return PgwConverter.toBytes(field,rs,i);
        /*try {
            if (PgwConverter.isByte(field.getColumnClassName())) {
                return rs.getBytes(i);
            } else {
                return rs.getString(i).getBytes(StandardCharsets.UTF_8);
            }
        }catch (Exception ex){
            throw new SQLException(ex);
        }*/
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




}
