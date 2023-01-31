package org.example.messages.sslnegotiation;

import org.example.messages.PGServerMessage;

import java.nio.ByteBuffer;

public class SSLResponse implements PGServerMessage {
    @Override
    public ByteBuffer encode() {
        ByteBuffer sslResponse = ByteBuffer.allocate(1);
        sslResponse.put((byte) 'N');
        sslResponse.flip();
        return sslResponse;
    }
}
