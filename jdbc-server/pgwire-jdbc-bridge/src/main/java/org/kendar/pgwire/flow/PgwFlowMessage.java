package org.kendar.pgwire.flow;

import org.kendar.pgwire.commons.Context;
import org.kendar.pgwire.commons.DataMessage;

import java.io.IOException;

public interface PgwFlowMessage {
    void read(DataMessage message) throws IOException;

    void handle(Context context) throws IOException;
}
