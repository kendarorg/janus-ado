package org.example.messages.extendedquery;

import org.example.messages.PGClientMessage;
import org.example.server.Context;

import java.nio.ByteBuffer;

public class DescribeMessage implements PGClientMessage {
    private char type;
    private String toDescribe;

    public DescribeMessage(){

    }
    public DescribeMessage(char type, String toDescribe) {

        this.type = type;
        this.toDescribe = toDescribe;
    }

    @Override
    public PGClientMessage decode(ByteBuffer buffer) {
        var prev= buffer.position();
        buffer.position(prev+1);
        var length = buffer.getInt();

        type = (char)buffer.get();
        toDescribe = PGClientMessage.extractString(buffer);
        return new DescribeMessage(type,toDescribe);
    }

    @Override
    public boolean isMatching(ByteBuffer b) {
        var pos = b.position();
        return b.get(pos+0)=='D';
    }

    @Override
    public void handle(Context client) {

    }
}
