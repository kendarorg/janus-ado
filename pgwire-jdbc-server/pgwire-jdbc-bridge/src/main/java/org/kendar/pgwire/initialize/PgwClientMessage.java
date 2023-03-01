package org.kendar.pgwire.initialize;

import org.kendar.pgwire.commons.PgwByteBuffer;

import java.io.IOException;

public interface PgwClientMessage {
    void read(PgwByteBuffer buffer) throws IOException;
}
