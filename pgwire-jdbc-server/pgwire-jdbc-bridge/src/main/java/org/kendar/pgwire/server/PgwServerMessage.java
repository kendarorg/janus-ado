package org.kendar.pgwire.server;

import org.kendar.pgwire.commons.PgwByteBuffer;

import java.io.IOException;

public interface PgwServerMessage {
    void write(PgwByteBuffer buffer) throws IOException;
}
