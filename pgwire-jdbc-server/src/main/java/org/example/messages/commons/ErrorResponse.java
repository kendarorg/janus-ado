package org.example.messages.commons;

import org.example.messages.PGServerMessage;

import java.nio.ByteBuffer;
import java.nio.charset.StandardCharsets;

public class ErrorResponse implements PGServerMessage {
    private String message;

    public ErrorResponse(String message){

        this.message = message;
    }
    @Override
    public ByteBuffer encode() {
        var s = "FATAL".getBytes(StandardCharsets.UTF_8);
        var m = message.getBytes(StandardCharsets.UTF_8);
        var buffer = ByteBuffer.allocate(1+4+s.length+m.length+2+2);
        buffer.put((byte) 'E'); // 'Z' for ReadyForQuery
        buffer.putInt(4+s.length+m.length+2+2); // Length
        buffer.put((byte) 'S'); // Transaction status indicator, 'I' for idle
        buffer.put( s);
        buffer.put((byte) 0);
        buffer.put((byte) 'M'); // Transaction status indicator, 'I' for idle
        buffer.put( m);
        buffer.put((byte) 0);
        buffer.flip();
        return buffer;
    }
}
