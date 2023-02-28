package org.kendar.pgwire.flow;

import org.kendar.pgwire.commons.Context;
import org.kendar.pgwire.commons.DataMessage;
import org.kendar.pgwire.server.ReadyForQuery;

import java.io.IOException;

public class SyncMessage implements PgwFlowMessage{
    @Override
    public void read(DataMessage message) throws IOException {

    }

    @Override
    public void handle(Context context) throws IOException {
        context.getBuffer().write(new ReadyForQuery());
    }
}
