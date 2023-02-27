package org.example.builders;

import org.example.SqlStringType;
import org.example.StringParser;
import org.example.messages.QueryMessage;
import org.example.messages.commons.ErrorResponse;
import org.example.messages.commons.ReadyForQuery;
import org.example.messages.extendedquery.BindMessage;
import org.example.messages.extendedquery.ParseMessage;
import org.example.messages.querymessage.CommandComplete;
import org.example.messages.querymessage.DataRow;
import org.example.messages.querymessage.Field;
import org.example.messages.querymessage.RowDescription;
import org.example.server.Context;

import java.nio.ByteBuffer;
import java.sql.*;
import java.util.ArrayList;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.Future;

public class RealResultBuilder {

    public static Future<Integer> executeQuery(Connection conn, String query,
                                               SqlStringType type, Context client,Future<Integer> prev) throws SQLException {
        switch (type) {
            case UNKNOWN:
            case NONE:
            case SELECT: {
                var st = conn.prepareStatement(query);
                var result = st.execute();
                if (!result) {
                    var updateCount = st.getUpdateCount();
                    CommandComplete commandComplete = new CommandComplete("RESULT " + updateCount);
                    return client.write(commandComplete,prev);
                } else {
                    return loadResultset(client, st,prev, "", "",0);
                }
            }
            case UPDATE:{
                var st = conn.prepareStatement(query);
                var updateCount = st.executeUpdate();
                CommandComplete commandComplete = new CommandComplete("RESULT " + updateCount);
                return client.write(commandComplete,prev);
            }
            case CALL:{
                var st = conn.prepareCall(query);
                var result = st.execute();
                if (!result) {
                    var updateCount = st.getUpdateCount();
                    CommandComplete commandComplete = new CommandComplete("RESULT " + updateCount);
                    return client.write(commandComplete,prev);
                } else {
                    return loadResultset(client, st,prev, "", "",0);
                }
            }
        }
        return null;
    }

    public static Future<Integer> buildRealResultQuery(QueryMessage queryMessage, Context client,Future<Integer> prev) {
        try {
            var conn = client.getConnection();
            var query = queryMessage.getQuery();
            var parsed = StringParser.getTypes(query);
            if (StringParser.isUnknown(parsed)) {
                return executeQuery(conn,query,SqlStringType.UNKNOWN,client,prev);
            } else {
                Future<Integer> rs=null;
                for (var single : parsed) {
                    var tmp = executeQuery(conn, single.getValue(), single.getType(),client,prev);
                    if(tmp!=null){
                        rs=tmp;
                    }
                }
                return rs;
            }
        } catch (SQLException e) {
            throw new RuntimeException(e);
        }
    }

    public static Future<Integer> buildRealResultPs(ParseMessage parseMessage, Context client,
                                                    Future<Integer> prev,
                                                    String psName,String portal, int maxRecords) throws SQLException {

        var resultSet = client.get(psName+"_"+portal+"_rs");
        Future<Integer> writeResult = null;
        if(resultSet==null) {


            var conn = client.getConnection();
            var query = parseMessage.getQuery();


            var parsed = StringParser.getTypes(query);
            if (StringParser.isUnknown(parsed) || StringParser.isMixed(parsed) || parsed.size()==1) {
                var rs = buildTheResultForSingleQuery(client, prev, psName, portal, maxRecords, conn, query);
//                var rfq = new ReadyForQuery();
//                rs = client.write(rfq,rs);
                return rs;
            } else {
                Future<Integer> rs=null;
                for (var single : parsed) {
                    var tmp = //executeQuery(conn, single.getValue(), single.getType(),client,prev);
                            buildTheResultForSingleQuery(client, prev, psName, portal, maxRecords, conn, single.getValue());
                    if(tmp!=null){
                        try {
                            tmp.get();
                        } catch (Exception e) {

                        }
                        rs=tmp;
                    }
                }
//                var rfq = new ReadyForQuery();
//                rs = client.write(rfq,rs);
                return rs;
            }


        }else{
            if(maxRecords==0){
                maxRecords=Integer.MAX_VALUE;
            }
            int count = 0;
            var rs = (ResultSet) client.get(psName+"_"+portal+"_rs");
            var fields = (ArrayList<Field>)client.get(psName+"_"+portal+"_fi");
            while (rs.next() && count<maxRecords) {
                count++;
                var byteRow = new ArrayList<ByteBuffer>();
                for (var i = 0; i < fields.size(); i++) {
                    byteRow.add(buildData(fields.get(i), rs, i + 1));
                }
                DataRow dataRow = new DataRow(byteRow, fields);
                writeResult = client.write(dataRow,prev);
            }

            client.put(psName+"_"+portal+"_rs",rs);
            CommandComplete commandComplete = new CommandComplete("SELECT " + count);
            return client.write(commandComplete,writeResult);
        }
    }

    private static Future<Integer> buildTheResultForSingleQuery(Context client, Future<Integer> prev, String psName, String portal, int maxRecords, Connection conn, String query) {
        try {
            if (query.startsWith(":JANUS:")) {
                return handleSpecialQuery(conn, query, client);
            }

            var result = false;
            Statement st = null;
            var bind = (BindMessage) client.get("bind_"+ psName +"_"+ portal);
            if (bind != null) {
                if (bind.getParameterValues().size() > 0) {
                    st = conn.prepareStatement(query);
                    var pmd = ((PreparedStatement) st).getParameterMetaData();
                    for (var i = 0; i < bind.getParameterValues().size(); i++) {
                        var clName = pmd.getParameterClassName(i + 1);
                        var clPrec = pmd.getPrecision(i + 1);
                        var clScale = pmd.getScale(i + 1);
                        var sqlType = pmd.getParameterType(i + 1);
                        Class<?> clReal;
                        try {
                            clReal = Class.forName(clName);
                            ((PreparedStatement) st).setObject(i + 1,
                                    PgwConverter.toPgWire(
                                            bind.getParamFormatCodes()[i],
                                            clReal,
                                            bind.getParameterValues().get(i),
                                            clPrec, clScale),
                                    sqlType, clScale);
                        } catch (Exception e) {
                            throw new RuntimeException(e);
                        }

                    }
                    result = ((PreparedStatement) st).execute();
                }
            }
            if (st == null) {
                st = conn.createStatement();
                result = st.execute(query);
            }
            if (result) {
                return loadResultset(client, st, prev, psName, portal, maxRecords);
            }else{
                var count = st.getUpdateCount();
                CommandComplete commandComplete = new CommandComplete("RESULT "+count);
                return client.write(commandComplete, prev);
            }
        } catch (SQLException e) {
            ErrorResponse errorResponse = new ErrorResponse(e.getMessage());
            prev = client.write(errorResponse, prev);
        }
        CommandComplete commandComplete = new CommandComplete(query);
        return client.write(commandComplete, prev);
    }


    private static Future<Integer> loadResultset(Context client, Statement st, Future<Integer> prev,
                                                 String psName, String portal, int maxRecords) throws SQLException {
        Future<Integer> writeResult;
        var rs = st.getResultSet();
        var md = rs.getMetaData();

        var fields = new ArrayList<Field>();
        for (var i = 0; i < md.getColumnCount(); i++) {
            fields.add(new Field(
                    md.getColumnName(i + 1),
                    0,
                    0, PgwConverter.toPgwType(md.getColumnType(i + 1))
                    , md.getPrecision(i + 1), -1, PgwConverter.isByteOut(md.getColumnClassName(i + 1)) ? 1 : 0
                    , md.getColumnClassName(i + 1), md.getScale(i + 1), md.getColumnType(i + 1)));
        }

        RowDescription rowDescription = new RowDescription(fields);
        writeResult = client.write(rowDescription,prev);
        if(maxRecords==0){
            maxRecords=Integer.MAX_VALUE;
        }
        int count = 0;
        while (rs.next() && count<maxRecords) {
            count++;
            var byteRow = new ArrayList<ByteBuffer>();
            for (var i = 0; i < fields.size(); i++) {
                byteRow.add(buildData(fields.get(i), rs, i + 1));
            }
            DataRow dataRow = new DataRow(byteRow, fields);
            writeResult = client.write(dataRow,writeResult);
        }
        client.put(psName+"_"+portal+"_rs",rs);
        client.put(psName+"_"+portal+"_fi",fields);
        CommandComplete commandComplete = new CommandComplete("SELECT " + count);
        return client.write(commandComplete,writeResult);
    }

    private static ByteBuffer buildData(Field field, ResultSet rs, int i) throws SQLException {
        return PgwConverter.toBytes(field, rs, i);
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
            if (conn.getAutoCommit()) conn.setAutoCommit(false);
        } else if (query.equalsIgnoreCase("JANUS:ROLLBACK_TRANSACTION")) {
            conn.rollback();
        } else if (query.equalsIgnoreCase("JANUS:COMMIT_TRANSACTION")) {
            conn.commit();
            conn.setAutoCommit(true);
        } else if (query.startsWith("JANUS:SET_SAVEPOINT:")) {
            if (conn.getAutoCommit()) conn.setAutoCommit(false);
            var val = query.split(":");
            if (val[2].isEmpty()) {
                var savepoint = conn.setSavepoint();
                client.put("savepoint",savepoint);
            } else {
                var savepoint = conn.setSavepoint(val[2]);
                client.put("savepoint_"+val[2],savepoint);
            }
        } else if (query.startsWith("JANUS:RELEASE_SAVEPOINT:")) {
            var val = query.split(":");

            Savepoint svp = getSavepoint(client, val);
            if (svp != null) {
                conn.releaseSavepoint(svp);
            }
        } else if (query.startsWith("JANUS:ROLLBACK_SAVEPOINT:")) {
            var val = query.split(":");

            Savepoint svp = getSavepoint(client, val);
            if (svp != null) {
                conn.rollback(svp);
            }
        }
        throw new RuntimeException("NOT IMPLEMENTED SPECIAL QUERIES");
    }

    private static Savepoint getSavepoint(Context client, String[] val) {
        if(val.length>=3){
            return (Savepoint)client.get("savepoint_"+val[2]);
        }else{
            return (Savepoint)client.get("savepoint");
        }
    }


}
