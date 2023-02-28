package org.kendar.pgwire.flow;

import org.kendar.pgwire.commons.Context;
import org.kendar.pgwire.commons.DataMessage;
import org.kendar.pgwire.executors.BaseExecutor;
import org.kendar.pgwire.executors.ExtendedFlowExecutor;
import org.kendar.pgwire.server.ReadyForQuery;
import org.kendar.pgwire.server.RowDescription;
import org.kendar.pgwire.utils.*;

import java.io.IOException;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.util.ArrayList;

public class DescribeMessage  implements PgwFlowMessage{




    private char type;

    public char getType() {
        return type;
    }

    public void setType(char type) {
        this.type = type;
    }

    public String getPortal() {
        return portal;
    }

    public void setPortal(String portal) {
        this.portal = portal;
    }

    private String portal;

    @Override
    public void read(DataMessage message) throws IOException {
        type = (char)message.readByte();
        portal = message.readString();
    }


    @Override
    public void handle(Context context) throws IOException {
        var executor = new ExtendedFlowExecutor();
        executor.handle(context,portal);

        if(context.get("result_" + portal)!=null && context.get("result_" + portal).getClass()==String.class){
            return;
        }
        try {
            if (type == 'P') {
                if(context.get("result_" + portal)==null){
                    context.getBuffer().write(new ReadyForQuery());
                }else {
                    var resultSet = (ResultSet) context.get("result_" + portal);
                    ArrayList<Field> fields = BaseExecutor.writeRowDescriptor(context, resultSet);
                    context.put("field_" + portal, fields);
                }
            } else if (type == 'S') {

            }
        }catch (SQLException ex){
            throw new IOException(ex);
        }
    }


}
