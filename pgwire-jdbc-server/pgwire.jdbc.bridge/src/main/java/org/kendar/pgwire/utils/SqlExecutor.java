package org.kendar.pgwire.utils;

import org.kendar.pgwire.commons.Context;
import org.kendar.pgwire.flow.BindMessage;
import org.kendar.pgwire.flow.ParseMessage;
import org.kendar.pgwire.flow.SyncMessage;
import org.kendar.pgwire.server.CommandComplete;
import org.kendar.pgwire.server.EmptyQueryResponse;
import org.kendar.pgwire.server.ReadyForQuery;

import java.io.IOException;
import java.sql.PreparedStatement;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.List;
import java.util.Locale;
import java.util.Set;
import java.util.concurrent.ConcurrentSkipListSet;

public class SqlExecutor {

    private static Set<String> fakeQueries;

    static {
        fakeQueries = new ConcurrentSkipListSet<>();
        fakeQueries.add("SET extra_float_digits".toLowerCase(Locale.ROOT));
        fakeQueries.add("SET application_name".toLowerCase(Locale.ROOT));
    }


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

    private boolean shouldHandleAsSingleQuery(List<SqlParseResult> parsed) {
        return StringParser.isUnknown(parsed) || StringParser.isMixed(parsed) || parsed.size() == 1;
    }

    private boolean hasBind(BindMessage bind) {
        return bind != null && bind.getParameterValues().size() > 0;
    }


    private void handleExecuteRequest(Context context, String statementName, BindMessage bind,
                                      ParseMessage statement,String portal) throws SQLException, IOException {
        var parsed = StringParser.getTypes(statement.getQuery());
        if (!shouldHandleAsSingleQuery(parsed)) {
            for (var singleQuery : parsed) {

            }
        } else {
            boolean result;
            var singleParsed = parsed.get(0);
            var singleQuery = singleParsed.getValue();
            var type = singleParsed.getType();
            var conn = context.getConnection();
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
                var resultSet= st.getResultSet();

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
}
