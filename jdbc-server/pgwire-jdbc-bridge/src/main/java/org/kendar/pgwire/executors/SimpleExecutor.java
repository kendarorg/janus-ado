package org.kendar.pgwire.executors;

import org.kendar.pgwire.PgwSocketHandler;
import org.kendar.pgwire.commons.Context;
import org.kendar.pgwire.flow.BindMessage;
import org.kendar.pgwire.flow.ParseMessage;
import org.kendar.pgwire.flow.SyncMessage;
import org.kendar.pgwire.server.*;
import org.kendar.pgwire.utils.Field;
import org.kendar.pgwire.utils.PgwConverter;
import org.kendar.pgwire.utils.SqlParseResult;
import org.kendar.pgwire.utils.StringParser;

import java.io.IOException;
import java.nio.ByteBuffer;
import java.nio.charset.StandardCharsets;
import java.sql.Connection;
import java.sql.SQLException;
import java.sql.Statement;
import java.sql.Types;
import java.util.ArrayList;
import java.util.Locale;

public class SimpleExecutor extends BaseExecutor{
    public void handle(Context context, String query) throws IOException, SQLException {
        if (query.trim().isEmpty()) {
            context.getBuffer().write(new EmptyQueryResponse());
            return;
        }

        if (fakeQueries.stream().anyMatch(a -> query.toLowerCase(Locale.ROOT).contains(a))) {

            if(query.equalsIgnoreCase("select oid, typbasetype from pg_type where typname = 'lo'")){
                handleOdbcStartQuery(context);
            }else if(query.contains("select nspname from pg_namespace")){
                handleSelectSpName(context);
            }else if(query.contains("select n.nspname, c.relname, a.attname, a.atttypid")){
                handleHuge(context);
            }else if(query.contains("select current_schema()")){
                handleSelectSpName(context);
            }else {
                try {
                    handleExecuteRequest(context, query);
                } catch (Exception ex) {
                    if (query.toLowerCase(Locale.ROOT).startsWith("select")||query.toLowerCase(Locale.ROOT).startsWith("set ")) {
                        context.getBuffer().write(new CommandComplete("SELECT 0 0"));
                    }else{
                        context.getBuffer().write(new CommandComplete("RESULT 0"));
                    }
                    context.getBuffer().write(new ReadyForQuery(context.inTransaction()));
                }
            }
        }else{
            handleExecuteRequest(context, query);
            context.getBuffer().write(new ReadyForQuery(context.inTransaction()));
        }
    }


    private void handleHuge(Context context) throws IOException {
        context.getBuffer().write(new CommandComplete("SELECT 0"));
        //context.getBuffer().write(new ErrorResponse("NOTHING FOUND"));
        context.getBuffer().write(new ReadyForQuery(false));
    }

    private void handleSelectSpName(Context context) throws SQLException, IOException {
        var fields = new ArrayList<Field>();
        fields.add(new Field(
                "nspname",
                0,
                0, PgwConverter.toPgwType(Types.VARCHAR)
                , 255, -1, 0
                , "java.lang.String", 1, Types.VARCHAR));
        context.getBuffer().write(new RowDescription(fields));
        var byteRow = new ArrayList<ByteBuffer>();
        byteRow.add(ByteBuffer.wrap("public".getBytes(StandardCharsets.UTF_8)));
        context.getBuffer().write(new DataRow(byteRow, fields));
        /*
        /*
  oid  | typbasetype
-------+-------------
 16385 |          26
(1 row)

 */
        context.getBuffer().write(new CommandComplete("SELECT 1"));
        //context.getBuffer().write(new ErrorResponse("NOTHING FOUND"));
        context.getBuffer().write(new ReadyForQuery(false));
    }

    private void handleOdbcStartQuery(Context context) throws SQLException, IOException {
        var fields = new ArrayList<Field>();
        fields.add(new Field(
                "oid",
                0,
                0, PgwConverter.toPgwType(Types.BIGINT)
                , 10, -1, 0
                , "java.lang.Long", 1, Types.BIGINT));
        fields.add(new Field(
                "typbasetype",
                0,
                0, PgwConverter.toPgwType(Types.BIGINT)
                , 10, -1, 0
                , "java.lang.Long", 1, Types.BIGINT));
        context.getBuffer().write(new RowDescription(fields));
        var byteRow = new ArrayList<ByteBuffer>();
        byteRow.add(ByteBuffer.wrap("16385".getBytes(StandardCharsets.UTF_8)));
        byteRow.add(ByteBuffer.wrap("26".getBytes(StandardCharsets.UTF_8)));
        context.getBuffer().write(new DataRow(byteRow, fields));
        /*
        /*
  oid  | typbasetype
-------+-------------
 16385 |          26
(1 row)

 */
        context.getBuffer().write(new CommandComplete("SELECT 1"));
        //context.getBuffer().write(new ErrorResponse("NOTHING FOUND"));
        context.getBuffer().write(new ReadyForQuery(false));
    }

    private void handleExecuteRequest(Context context, String query) throws SQLException, IOException {

        var conn = context.getConnection();

        query = handleOdbcTransactions(context, conn, query);
        if(query==null)return;
        var parsed = StringParser.getTypes(query);
        if (!shouldHandleAsSingleQuery(parsed)) {

            if(!context.inTransaction()) {
                conn.setAutoCommit(false);
                context.setTransaction(true);
            }
            for (var singleQuery : parsed) {
                executeSingleQuery(context,singleQuery);
            }

            if(!context.inTransaction()) {
                context.setTransaction(false);
                conn.commit();
                conn.setAutoCommit(true);
            }
        } else {
            var singleParsed = parsed.get(0);

            executeSingleQuery(context, singleParsed);
        }
    }

    private void executeSingleQuery(Context context, SqlParseResult singleParsed) throws SQLException, IOException {
        boolean result;
        var singleQuery = singleParsed.getValue();
        var type = singleParsed.getType();
        var conn = context.getConnection();



        if (singleQuery.startsWith("JANUS:") && context.isJanus()) {
            handleSpecialQuery(context,conn, singleQuery);
            return;
        }
        Statement st;

        st = conn.createStatement();
        result = st.execute(singleQuery);

        if(result){
            var resultSet= st.getResultSet();
            ArrayList<Field> fields = writeRowDescriptor(context, resultSet);
            var count = sendDataRowsCount(context,resultSet,0,fields);
            context.getBuffer().write (new CommandComplete("SELECT "+count));
        }else{
            var count = st.getUpdateCount();
            switch(type){
                case INSERT:
                    context.getBuffer().write (new CommandComplete("INSERT 0 "+count));
                    break;
                case UPDATE:
                    context.getBuffer().write (new CommandComplete("UPDATE "+count));
                    break;
                default:
                    context.getBuffer().write (new CommandComplete("SELECT "+count));
                    break;
            }
        }
    }
}
