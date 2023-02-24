package org.example.messages;

import org.example.messages.commons.ReadyForQuery;
import org.example.server.Context;

import java.nio.ByteBuffer;
import java.util.concurrent.Future;

public class SyncMessage implements PGClientMessage{
    public SyncMessage(){

    }

    @Override
    public PGClientMessage decode(ByteBuffer buffer) {
        buffer.get();
        buffer.getInt();
        return new SyncMessage();
    }

    @Override
    public boolean isMatching(ByteBuffer b) {
        var pos = b.position();
        if(b.limit()<pos+1)return false;
        return b.get(pos+0)=='S';
    }

    @Override
    public void handle(Context client,Future<Integer> prev) {
        Future<Integer> writeResult;
        ReadyForQuery readyForQuery = new ReadyForQuery();
        writeResult = client.write(readyForQuery,prev);
        try {
            writeResult.get();
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
