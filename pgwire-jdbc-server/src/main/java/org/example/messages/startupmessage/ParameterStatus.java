package org.example.messages.startupmessage;

import org.example.messages.PGServerMessage;

import java.nio.ByteBuffer;
import java.nio.charset.StandardCharsets;

public class ParameterStatus implements PGServerMessage {
    private String key;
    private String value;

    public ParameterStatus(String key, String value) {

        this.key = key;
        this.value = value;
    }

    @Override
    public ByteBuffer encode() {
        var k=key.getBytes(StandardCharsets.UTF_8);
        var v=value.getBytes(StandardCharsets.UTF_8);
        var buffer = ByteBuffer.allocate(1+4+k.length+v.length+2);
        buffer.put((byte) 'S'); // 'R' for AuthenticationRequest
        buffer.putInt(key.length()+value.length()+2+4); // Length
        buffer.put(k);
        buffer.put((byte) 0);
        buffer.put(v);
        buffer.put((byte) 0);
        buffer.flip();
        return buffer;
    }
}
