package org.kendar.pgwire.server;

import org.kendar.pgwire.commons.PgwByteBuffer;

import java.io.IOException;

public class EmptyQueryResponse implements PgwServerMessage{
    @Override
    public void write(PgwByteBuffer buffer) throws IOException {
        buffer.writeByte((byte) 'I');
        buffer.writeInt(4);
    }
}
