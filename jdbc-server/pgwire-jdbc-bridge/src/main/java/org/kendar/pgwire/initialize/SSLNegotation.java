package org.kendar.pgwire.initialize;

import org.kendar.pgwire.commons.PgwByteBuffer;
import org.kendar.pgwire.server.PgwServerMessage;
import org.kendar.pgwire.server.SSLResponse;

import java.io.IOException;

public class SSLNegotation implements PgwClientMessage{
    private final int length;

    public SSLNegotation(int firstLength) {
        length=firstLength;
    }

    @Override
    public void read(PgwByteBuffer buffer) throws IOException {
        //var length = buffer.readInt32(); //112-8
        if(length!=8)throw new IOException("Wrong SSLNegotation length");
        var sslcode = buffer.readInt32();
        if(sslcode!=80877103)throw new IOException("Wrong SSLNegotation SSL code");
        buffer.write((PgwServerMessage)new SSLResponse());
    }
}
