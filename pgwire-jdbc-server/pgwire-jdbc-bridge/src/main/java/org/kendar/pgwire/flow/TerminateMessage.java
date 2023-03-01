package org.kendar.pgwire.flow;

import org.kendar.pgwire.commons.Context;
import org.kendar.pgwire.commons.DataMessage;
import org.kendar.pgwire.server.ReadyForQuery;

import java.io.IOException;
import java.sql.SQLException;

public class TerminateMessage implements PgwFlowMessage{
    @Override
    public void read(DataMessage message) throws IOException {

    }

    @Override
    public void handle(Context context) throws IOException {

            context.close();
    }
}
