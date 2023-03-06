package org.kendar.pgwire.flow;

import org.kendar.pgwire.commons.Context;
import org.kendar.pgwire.commons.DataMessage;
import org.kendar.pgwire.executors.BaseExecutor;
import org.kendar.pgwire.executors.ExtendedFlowExecutor;
import org.kendar.pgwire.server.CommandComplete;
import org.kendar.pgwire.server.DataRow;
import org.kendar.pgwire.utils.*;

import java.io.IOException;
import java.nio.ByteBuffer;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.ArrayList;

public class ExecuteMessage implements PgwFlowMessage{
    private String portal;
    private int maxRecords;

    @Override
    public void read(DataMessage message) throws IOException {
        portal = message.readString();
        maxRecords = message.readInt();
    }

    @Override
    public void handle(Context context) throws IOException {
        var dt = context.get("result_"  + portal);
        if(dt==null){
            var executor = new ExtendedFlowExecutor();
            executor.handle(context,portal);
        }else if(dt.getClass()==String.class){
            context.put("result_"  + portal,null);
            context.getBuffer().write(new CommandComplete((String)dt));
        }else  {
            var rs = (ResultSet) dt;
            var max = maxRecords;
            try {
                var fields =(ArrayList<Field>) context.get("field_"+portal);
                BaseExecutor.sendDataRows(context, rs, max, fields);
            }catch (Exception ex){

            }
        }
    }




}
