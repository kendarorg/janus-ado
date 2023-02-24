package org.example.messages;

import org.example.messages.sslnegotiation.SSLResponse;
import org.example.server.Context;

import java.nio.ByteBuffer;
import java.util.concurrent.Future;

public class TerminateMessage implements PGClientMessage {
    public TerminateMessage(){

    }

    @Override
    public PGClientMessage decode(ByteBuffer source) {

        return new TerminateMessage();
    }

    @Override
    public boolean isMatching(ByteBuffer b) {
        var pos = b.position();
        if(b.limit()<pos+1)return false;
        return b.get(pos+0)=='X';
    }

    @Override
    public void handle(Context client, Future<Integer> prev) {

    }
}
