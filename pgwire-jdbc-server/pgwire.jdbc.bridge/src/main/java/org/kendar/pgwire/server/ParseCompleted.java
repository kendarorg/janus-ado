package org.kendar.pgwire.server;

import org.kendar.pgwire.commons.PgwByteBuffer;

import java.io.IOException;

public class ParseCompleted  implements PgwServerMessage{
    @Override
    public void write(PgwByteBuffer buffer) throws IOException {
        buffer.writeByte((byte) '1');
        buffer.writeInt(4);
    }
}

