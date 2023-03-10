package org.kendar.pgwire.executors;

import org.kendar.pgwire.commons.Context;
import org.kendar.pgwire.flow.SyncMessage;
import org.kendar.pgwire.server.CommandComplete;
import org.kendar.pgwire.server.DataRow;
import org.kendar.pgwire.server.ReadyForQuery;
import org.kendar.pgwire.server.RowDescription;
import org.kendar.pgwire.utils.Field;
import org.kendar.pgwire.utils.PgwConverter;
import org.kendar.pgwire.utils.SqlParseResult;
import org.kendar.pgwire.utils.StringParser;

import java.io.IOException;
import java.nio.ByteBuffer;
import java.sql.Connection;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Savepoint;
import java.util.ArrayList;
import java.util.List;
import java.util.Locale;
import java.util.Set;
import java.util.concurrent.ConcurrentSkipListSet;

public class BaseExecutor {
    protected static Set<String> fakeQueries;

    static {
        fakeQueries = new ConcurrentSkipListSet<>();
        fakeQueries.add("SET extra_float_digits".toLowerCase(Locale.ROOT));
        fakeQueries.add("SET application_name".toLowerCase(Locale.ROOT));
        fakeQueries.add("select oid, typbasetype from pg_type where typname = 'lo'".toLowerCase(Locale.ROOT));
        fakeQueries.add("select nspname from pg_namespace".toLowerCase(Locale.ROOT));
        fakeQueries.add("select n.nspname, c.relname, a.attname, a.atttypid".toLowerCase(Locale.ROOT));
        fakeQueries.add("set client_encoding".toLowerCase(Locale.ROOT));
        fakeQueries.add("set statement_timeout".toLowerCase(Locale.ROOT));
        fakeQueries.add("select current_schema()".toLowerCase(Locale.ROOT));




        //fakeQueries.add("SET statement_timeout = 0".toLowerCase(Locale.ROOT));



    }

    protected boolean shouldHandleAsSingleQuery(List<SqlParseResult> parsed) {
        return StringParser.isUnknown(parsed) || StringParser.isMixed(parsed) || parsed.size() == 1;
    }

    public static ArrayList<Field> writeRowDescriptor(Context context, ResultSet resultSet) throws SQLException, IOException {
        var resultSetMetaData = resultSet.getMetaData();
        var fields = new ArrayList<Field>();
        for (var i = 0; i < resultSetMetaData.getColumnCount(); i++) {
            fields.add(new Field(
                    resultSetMetaData.getColumnName(i + 1),
                    0,
                    0, PgwConverter.toPgwType(resultSetMetaData.getColumnType(i + 1))
                    , resultSetMetaData.getPrecision(i + 1), -1, PgwConverter.isByteOut(resultSetMetaData.getColumnClassName(i + 1)) ? 1 : 0
                    , resultSetMetaData.getColumnClassName(i + 1), resultSetMetaData.getScale(i + 1), resultSetMetaData.getColumnType(i + 1)));
        }
        context.getBuffer().write(new RowDescription(fields));
        return fields;
    }


    private static ByteBuffer buildData(Field field, ResultSet rs, int i) throws SQLException {
        return PgwConverter.toBytes(field, rs, i);
    }

    public static void sendDataRows(Context context, ResultSet rs, int max,  ArrayList<Field> fields) throws SQLException, IOException {
        int count = sendDataRowsCount(context, rs, max, fields);
        if(max ==0){
            context.getBuffer().write(new CommandComplete("SELECT "+ count));
            var sync = context.waitFor('S');
            var sm = new SyncMessage();
            sm.read(sync);
            sm.handle(context);
        }
    }

    public static int sendDataRowsCount(Context context, ResultSet rs, int max, ArrayList<Field> fields) throws SQLException, IOException {
        int count=0;
        while (rs.next() && (count < max || max ==0)) {
            count++;
            var byteRow = new ArrayList<ByteBuffer>();
            for (var i = 0; i < fields.size(); i++) {
                byteRow.add(buildData(fields.get(i), rs, i + 1));
            }
            context.getBuffer().write(new DataRow(byteRow, fields));
        }
        return count;
    }



    protected void handleSpecialQuery(Context context, Connection conn, String query) throws SQLException {
        try {
            if (query.equalsIgnoreCase("JANUS:BEGIN_TRANSACTION")) {
                beginTransaction(context, conn);
                //context.getBuffer().write(new ReadyForQuery(true));

            } else if (query.equalsIgnoreCase("JANUS:ROLLBACK_TRANSACTION")) {
                rollbackTransaction(context, conn);
                //context.getBuffer().write(new ReadyForQuery(false));
            } else if (query.equalsIgnoreCase("JANUS:COMMIT_TRANSACTION")) {
                commitTransaction(context, conn);
                //context.getBuffer().write(new ReadyForQuery(false));
            } else if (query.startsWith("JANUS:SET_SAVEPOINT:")) {
                var val = query.split(":");
                if (val[2].isEmpty()) {
                    var savepoint = conn.setSavepoint();
                    context.put("savepoint_null", savepoint);
                } else {
                    var savepoint = conn.setSavepoint(val[2]);
                    context.put("savepoint_" + val[2], savepoint);
                }
                context.getBuffer().write(new CommandComplete("RESULT 0"));
                //context.getBuffer().write(new ReadyForQuery(false));
            } else if (query.startsWith("JANUS:RELEASE_SAVEPOINT:")) {
                var val = query.split(":");

                Savepoint svp = getSavepoint(context, val);
                if (svp != null) {
                    conn.releaseSavepoint(svp);
                }
                context.getBuffer().write(new CommandComplete("RESULT 0"));
                //context.getBuffer().write(new ReadyForQuery(false));
            } else if (query.startsWith("JANUS:ROLLBACK_SAVEPOINT:")) {
                var val = query.split(":");

                Savepoint svp = getSavepoint(context, val);
                if (svp != null) {
                    conn.rollback(svp);
                }
                context.getBuffer().write(new CommandComplete("RESULT 0"));
                //context.getBuffer().write(new ReadyForQuery(false));
            } else {
                throw new SQLException("NOT IMPLEMENTED SPECIAL QUERIES");
            }
        }catch (IOException e){
            throw new SQLException(e);
        }
    }

    protected void commitTransaction(Context context, Connection conn) throws SQLException, IOException {
        conn.commit();
        context.setTransaction(false);
        conn.setAutoCommit(true);
        context.getBuffer().write(new CommandComplete("COMMIT"));
    }

    protected void rollbackTransaction(Context context, Connection conn) throws SQLException, IOException {
        conn.rollback();
        context.setTransaction(false);
        conn.setAutoCommit(true);
        context.getBuffer().write(new CommandComplete("ROLLBACK"));
    }

    protected void beginTransaction(Context context, Connection conn) throws SQLException, IOException {
        if (conn.getAutoCommit()) conn.setAutoCommit(false);
        context.setTransaction(true);
        context.getBuffer().write(new CommandComplete("UPDATE 0"));
    }


    private static Savepoint getSavepoint(Context client, String[] val) {
        if(val.length>=3){
            return (Savepoint)client.get("savepoint_"+val[2]);
        }else{
            return (Savepoint)client.get("savepoint_null");
        }
    }


    protected String handleOdbcTransactions(Context context, Connection conn, String query) throws SQLException, IOException {
        if(context.isJanus()){
            return query;
        }
        if (query.startsWith("BEGIN;") && query.length()>"BEGIN;".length()) {
            //To handle odbc
            if(!context.inTransaction()) {
                //conn.commit();
                beginTransaction(context, conn);
                context.getBuffer().write(new ReadyForQuery(context.inTransaction()));
                query = query.substring("BEGIN;".length());
                System.out.println("[SERVER] ODBC Transaction begin");
            }else{
                query = query.substring("BEGIN;".length());
            }
        }else if (query.equalsIgnoreCase("COMMIT")) {
            if(context.inTransaction()) {
                commitTransaction(context, conn);
            }else{
                context.getBuffer().write(new CommandComplete("COMMIT"));
            }
            //context.getBuffer().write(new ReadyForQuery(context.inTransaction()));
            return null;
        }else if (query.startsWith("SAVEPOINT ")&&query.indexOf(';')>0){
            var index = query.indexOf(';');
            query = query.substring(index+1);
        }
        return query;
    }
}
