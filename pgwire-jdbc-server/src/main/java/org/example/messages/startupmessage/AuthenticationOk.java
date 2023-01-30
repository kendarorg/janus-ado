package org.example.messages.startupmessage;

import org.example.messages.PGServerMessage;

import java.nio.ByteBuffer;

public class AuthenticationOk implements PGServerMessage {
    public AuthenticationOk(){

    }

    @Override
    public ByteBuffer encode() {
        var buffer = ByteBuffer.allocate(9);
        buffer.put((byte) 'R'); // 'R' for AuthenticationRequest
        buffer.putInt(8); // Length
        buffer.putInt(0); // Authentication type, 0 for OK
        buffer.flip();
        return buffer;
    }
}
