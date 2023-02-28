package org.kendar.pgwire.flow;

import org.kendar.pgwire.commons.Context;
import org.kendar.pgwire.commons.DataMessage;
import org.kendar.pgwire.server.CommandComplete;
import org.kendar.pgwire.server.DataRow;
import org.kendar.pgwire.server.EmptyQueryResponse;
import org.kendar.pgwire.server.ReadyForQuery;
import org.kendar.pgwire.utils.*;

import java.io.IOException;
import java.nio.ByteBuffer;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.ArrayList;
import java.util.List;
import java.util.Locale;
import java.util.Set;
import java.util.concurrent.ConcurrentSkipListSet;

public class ExecuteMessage implements PgwFlowMessage{
    private String portal;
    private int maxRecords;

    @Override
    public void read(DataMessage message) throws IOException {
        portal = message.readString();
        maxRecords = message.readInt();
    }
    private static ByteBuffer buildData(Field field, ResultSet rs, int i) throws SQLException {
        return PgwConverter.toBytes(field, rs, i);
    }

    @Override
    public void handle(Context context) throws IOException {
        var dt = context.get("result_"  + portal);
        if(dt==null){
            var executor = new SqlExecutor();
            executor.handle(context,portal);
        }else if(dt.getClass()==String.class){
            context.put("result_"  + portal,null);
            context.getBuffer().write(new CommandComplete((String)dt));
        }else  {
            var rs = (ResultSet) dt;
            try {
                int count = 0;
                var fields =(ArrayList<Field>) context.get("field_"+portal);
                while (rs.next() && (count < maxRecords ||maxRecords==0)) {
                    count++;
                    var byteRow = new ArrayList<ByteBuffer>();
                    for (var i = 0; i < fields.size(); i++) {
                        byteRow.add(buildData(fields.get(i), rs, i + 1));
                    }
                    context.getBuffer().write(new DataRow(byteRow, fields));
                }
                if(maxRecords==0){
                    context.getBuffer().write(new CommandComplete("SELECT "+count));
                    var sync = context.waitFor('S');
                    var sm = new SyncMessage();
                    sm.read(sync);
                    sm.handle(context);
                }
            }catch (Exception ex){

            }
        }
    }


}
