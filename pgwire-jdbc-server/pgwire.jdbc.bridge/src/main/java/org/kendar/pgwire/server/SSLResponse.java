package org.kendar.pgwire.server;

import org.kendar.pgwire.commons.PgwByteBuffer;

import java.io.IOException;

public class SSLResponse implements PgwServerMessage{
    @Override
    public void write(PgwByteBuffer pgwByteBuffer) throws IOException {
        pgwByteBuffer.writeByte((byte) 'N');
        //pgwByteBuffer.writeInt(4);
    }
}
