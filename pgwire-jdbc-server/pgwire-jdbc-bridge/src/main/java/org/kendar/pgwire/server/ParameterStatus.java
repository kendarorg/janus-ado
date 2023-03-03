package org.kendar.pgwire.server;

import org.kendar.pgwire.commons.PgwByteBuffer;

import java.io.IOException;
import java.nio.charset.StandardCharsets;

public class ParameterStatus implements PgwServerMessage{
    private String key;
    private String value;

    public String getKey() {
        return key;
    }

    public void setKey(String key) {
        this.key = key;
    }

    public String getValue() {
        return value;
    }

    public void setValue(String value) {
        this.value = value;
    }

    public ParameterStatus(String key, String value) {

        this.key = key;
        this.value = value;
    }

    @Override
    public void write(PgwByteBuffer buffer) throws IOException {
        var k=key.getBytes(StandardCharsets.UTF_8);
        var v=value.getBytes(StandardCharsets.UTF_8);
        buffer.writeByte((byte) 'S'); // 'R' for AuthenticationRequest
        buffer.writeInt(key.length()+value.length()+2+4); // Length
        buffer.write(k);
        buffer.writeByte((byte) 0);
        buffer.write(v);
        buffer.writeByte((byte) 0);
    }
}
