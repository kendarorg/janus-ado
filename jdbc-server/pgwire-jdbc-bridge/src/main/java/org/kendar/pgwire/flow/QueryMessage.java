package org.kendar.pgwire.flow;

import org.kendar.pgwire.commons.Context;
import org.kendar.pgwire.commons.DataMessage;
import org.kendar.pgwire.executors.SimpleExecutor;

import java.io.IOException;
import java.sql.SQLException;

public class QueryMessage implements PgwFlowMessage{
    private String query;

    @Override
    public void read(DataMessage message) throws IOException {
        query = message.readStringUtf8();
        System.out.println("[SERVER] Query: "+query);
    }

    @Override
    public void handle(Context context) throws IOException {
        var simple = new SimpleExecutor();
        try {
            simple.handle( context,query);
        } catch (SQLException e) {
            throw new IOException(e);
        }
    }
}
