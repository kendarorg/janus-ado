package org.kendar.pgwire.flow;

import org.kendar.pgwire.commons.Context;
import org.kendar.pgwire.commons.DataMessage;
import org.kendar.pgwire.server.CommandComplete;
import org.kendar.pgwire.server.EmptyQueryResponse;
import org.kendar.pgwire.server.ReadyForQuery;
import org.kendar.pgwire.server.RowDescription;
import org.kendar.pgwire.utils.*;

import java.io.IOException;
import java.sql.PreparedStatement;
import java.sql.ResultSet;
import java.sql.SQLException;
import java.sql.Statement;
import java.util.ArrayList;
import java.util.List;
import java.util.Locale;
import java.util.Set;
import java.util.concurrent.ConcurrentSkipListSet;

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
        var executor = new SqlExecutor();
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
                    context.put("field_" + portal, fields);
                }
            } else if (type == 'S') {

            }
        }catch (SQLException ex){
            throw new IOException(ex);
        }
    }
}
