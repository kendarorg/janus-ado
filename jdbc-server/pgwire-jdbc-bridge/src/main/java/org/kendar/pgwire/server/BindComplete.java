package org.kendar.pgwire.server;

import org.kendar.pgwire.commons.PgwByteBuffer;

import java.io.IOException;

public class BindComplete implements PgwServerMessage{
    @Override
    public void write(PgwByteBuffer buffer) throws IOException {
        buffer.writeByte((byte) '2');
        buffer.writeInt(4);
    }
}
