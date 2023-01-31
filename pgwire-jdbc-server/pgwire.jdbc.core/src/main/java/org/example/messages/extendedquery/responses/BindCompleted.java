package org.example.messages.extendedquery.responses;

import org.example.messages.PGServerMessage;

import java.nio.ByteBuffer;

public class BindCompleted implements PGServerMessage {
    @Override
    public ByteBuffer encode() {
        ByteBuffer sslResponse = ByteBuffer.allocate(5);
        sslResponse.put((byte) '2');
        sslResponse.putInt(4);
        sslResponse.flip();
        return sslResponse;
    }
}
