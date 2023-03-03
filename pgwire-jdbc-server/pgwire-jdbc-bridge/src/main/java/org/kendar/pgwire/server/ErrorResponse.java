package org.kendar.pgwire.server;

import org.kendar.pgwire.commons.PgwByteBuffer;

import java.io.IOException;
import java.nio.ByteBuffer;
import java.nio.charset.StandardCharsets;

public class ErrorResponse  implements PgwServerMessage{
    private String message;

    public ErrorResponse(String message){

        this.message = message;
    }
    @Override
    public void write(PgwByteBuffer buffer) throws IOException {
        var s = "FATAL".getBytes(StandardCharsets.UTF_8);
        var m = message.getBytes(StandardCharsets.UTF_8);
        
        buffer.writeByte((byte) 'E'); // 'Z' for ReadyForQuery
        buffer.writeInt(4+s.length+m.length+2+2); // Length
        buffer.writeByte((byte) 'S'); // severity
        buffer.write( s);
        buffer.writeByte((byte) 0);
        buffer.writeByte((byte) 'M'); // type
        buffer.write( m);
        buffer.writeByte((byte) 0);
    }
}
