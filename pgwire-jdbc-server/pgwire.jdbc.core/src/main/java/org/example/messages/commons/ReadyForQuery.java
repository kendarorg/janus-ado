package org.example.messages.commons;

import org.example.messages.PGServerMessage;

import java.nio.ByteBuffer;

public class ReadyForQuery implements PGServerMessage {
    public ReadyForQuery(){

    }

    @Override
    public ByteBuffer encode() {
        var buffer = ByteBuffer.allocate(6);
        buffer.put((byte) 'Z'); // 'Z' for ReadyForQuery
        buffer.putInt(5); // Length
        buffer.put((byte) 'I'); // Transaction status indicator, 'I' for idle
        buffer.flip();
        return buffer;
    }
}
