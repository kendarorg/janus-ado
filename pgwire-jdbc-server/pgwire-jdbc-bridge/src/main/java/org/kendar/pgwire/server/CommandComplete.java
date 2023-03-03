package org.kendar.pgwire.server;

import org.kendar.pgwire.commons.PgwByteBuffer;

import java.io.IOException;
import java.nio.charset.StandardCharsets;

public class CommandComplete implements PgwServerMessage{
    private String tag;

    public CommandComplete(String tag) {
        this.tag = tag;
    }

    @Override
    public void write(PgwByteBuffer buffer) throws IOException {
        var length = 4 + tag.length() + 1;
        buffer.writeByte((byte) 'C');
        buffer.writeInt(length);
        buffer.write(tag.getBytes(StandardCharsets.UTF_8));
        buffer.writeByte((byte)0);
    }
}
