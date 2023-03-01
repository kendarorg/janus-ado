package org.kendar.pgwire.executors;

import org.kendar.pgwire.commons.Context;
import org.kendar.pgwire.flow.BindMessage;
import org.kendar.pgwire.flow.ParseMessage;
import org.kendar.pgwire.flow.SyncMessage;
import org.kendar.pgwire.server.CommandComplete;
import org.kendar.pgwire.server.EmptyQueryResponse;
import org.kendar.pgwire.server.ErrorResponse;
import org.kendar.pgwire.utils.PgwConverter;
import org.kendar.pgwire.utils.SqlParseResult;
import org.kendar.pgwire.utils.StringParser;

import java.io.IOException;
import java.sql.PreparedStatement;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.List;
import java.util.Locale;
import java.util.Set;
import java.util.concurrent.ConcurrentSkipListSet;

public class ExtendedFlowExecutor extends BaseExecutor {



    public void handle(Context context,String portal){
        try {
            var statementName = (String) context.get("bind_statement_" + portal);
            var bind = (BindMessage) context.get("bind_" + statementName + "_" + portal);
            var statement = (ParseMessage) context.get("statement_" + statementName);

            if (statement.getQuery().trim().isEmpty()) {
                context.getBuffer().write(new EmptyQueryResponse());
                return;
            }

            if (fakeQueries.stream().anyMatch(a -> statement.getQuery().toLowerCase(Locale.ROOT).startsWith(a))) {

                try{
                    handleExecuteRequest(context, statementName, bind, statement,portal);
                }catch(Exception ex){
                    context.getBuffer().write(new CommandComplete("RESULT 0 "));
                    var sync = context.waitFor('S');
                    var sm = new SyncMessage();
                    sm.read(sync);
                    sm.handle(context);
                }
            }else{
                handleExecuteRequest(context, statementName, bind, statement,portal);
            }

        }catch (Exception ex){
            ex.printStackTrace();
            try {
                context.getBuffer().write(new ErrorResponse(ex.getMessage()));
            } catch (IOException e) {

            }
        }
    }

    private void loadBindParameters(BindMessage bind, PreparedStatement ps) throws SQLException {
        var pmd = ps.getParameterMetaData();
        for (var i = 0; i < bind.getParameterValues().size(); i++) {
            var clName = pmd.getParameterClassName(i + 1);
            var clPrec = pmd.getPrecision(i + 1);
            var clScale = pmd.getScale(i + 1);
            var sqlType = pmd.getParameterType(i + 1);
            Class<?> clReal;
            try {
                clReal = Class.forName(clName);
                ps.setObject(i + 1,
                        PgwConverter.toPgWire(
                                bind.getFormatCodes().get(i),
                                clReal,
                                bind.getParameterValues().get(i),
                                clPrec, clScale),
                        sqlType, clScale);
            } catch (Exception e) {
                throw new SQLException("Invalid type encountered",e);
            }
        }
    }



    private boolean hasBind(BindMessage bind) {
        return bind != null && bind.getParameterValues().size() > 0;
    }


    private void handleExecuteRequest(Context context, String statementName, BindMessage bind,
                                      ParseMessage statement,String portal) throws SQLException, IOException {
        var parsed = StringParser.getTypes(statement.getQuery());
        if (!shouldHandleAsSingleQuery(parsed)) {
            var conn = context.getConnection();
            if(!context.inTransaction()) {
                conn.setAutoCommit(false);
                context.setTransaction(true);
            }
            for (var singleQuery : parsed) {
                handleSingleQuery(context, bind, portal, singleQuery);
            }
            if(!context.inTransaction()) {
                context.setTransaction(false);
                conn.commit();
                conn.setAutoCommit(true);
            }
        } else {
            var singleParsed = parsed.get(0);


            handleSingleQuery(context, bind, portal, singleParsed);
        }
    }

    private void handleSingleQuery(Context context, BindMessage bind, String portal, SqlParseResult singleParsed) throws SQLException {
        boolean result;
        var singleQuery = singleParsed.getValue();
        var type = singleParsed.getType();
        var conn = context.getConnection();
        if (singleQuery.startsWith("JANUS:")) {
            handleSpecialQuery(context,conn, singleQuery);
            return;
        }
        Statement st;
        context.put("connection_"  + portal, conn);
        if (hasBind(bind)) {
            var ps = conn.prepareStatement(singleQuery);
            loadBindParameters(bind, ps);
            result = ps.execute();
            st =ps;
        }else{
            st = conn.createStatement();
            result = st.execute(singleQuery);
        }
        if(result){
            context.put("result_"  + portal, st.getResultSet());
        }else{
            var count = st.getUpdateCount();
            switch(type){
                case INSERT:
                    context.put("result_"  + portal, ("INSERT 0 "+count));
                    break;
                case UPDATE:
                    context.put("result_"  + portal, ("UPDATE "+count));
                    break;
                default:
                    context.put("result_"  + portal, ("SELECT 0 "+count));
                    break;
            }
        }
    }
}
