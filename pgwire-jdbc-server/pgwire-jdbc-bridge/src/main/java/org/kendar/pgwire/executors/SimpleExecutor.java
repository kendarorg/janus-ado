package org.kendar.pgwire.executors;

import org.kendar.pgwire.commons.Context;
import org.kendar.pgwire.flow.BindMessage;
import org.kendar.pgwire.flow.ParseMessage;
import org.kendar.pgwire.flow.SyncMessage;
import org.kendar.pgwire.server.CommandComplete;
import org.kendar.pgwire.server.EmptyQueryResponse;
import org.kendar.pgwire.server.ReadyForQuery;
import org.kendar.pgwire.utils.Field;
import org.kendar.pgwire.utils.SqlParseResult;
import org.kendar.pgwire.utils.StringParser;

import java.io.IOException;
import java.sql.Connection;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.ArrayList;
import java.util.Locale;

public class SimpleExecutor extends BaseExecutor{
    public void handle(Context context, String query) throws IOException, SQLException {
        if (query.trim().isEmpty()) {
            context.getBuffer().write(new EmptyQueryResponse());
            return;
        }

        if (fakeQueries.stream().anyMatch(a -> query.toLowerCase(Locale.ROOT).startsWith(a))) {

            try{
                handleExecuteRequest(context, query);
            }catch(Exception ex){
                context.getBuffer().write(new CommandComplete("RESULT 0 "));
                context.getBuffer().write(new ReadyForQuery(context.inTransaction()));
            }
        }else{
            handleExecuteRequest(context, query);
            context.getBuffer().write(new ReadyForQuery(context.inTransaction()));
        }
    }

    private void handleExecuteRequest(Context context, String query) throws SQLException, IOException {
        var parsed = StringParser.getTypes(query);
        if (!shouldHandleAsSingleQuery(parsed)) {
            var conn = context.getConnection();

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
        if (singleQuery.startsWith("JANUS:")) {
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
