package org.kendar.pgwire.executors;

import org.kendar.pgwire.commons.Context;
import org.kendar.pgwire.flow.SyncMessage;
import org.kendar.pgwire.server.CommandComplete;
import org.kendar.pgwire.server.DataRow;
import org.kendar.pgwire.server.RowDescription;
import org.kendar.pgwire.utils.Field;
import org.kendar.pgwire.utils.PgwConverter;
import org.kendar.pgwire.utils.SqlParseResult;
import org.kendar.pgwire.utils.StringParser;

import java.io.IOException;
import java.nio.ByteBuffer;
import java.sql.ResultSet;
import java.sql.SQLException;
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

}
