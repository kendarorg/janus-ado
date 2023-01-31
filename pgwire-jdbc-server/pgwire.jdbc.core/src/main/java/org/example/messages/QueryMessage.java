package org.example.messages;

import org.example.builders.FakeResultBuilder;
import org.example.PgWireFakeServer;
import org.example.builders.RealResultBuilder;
import org.example.messages.commons.ReadyForQuery;
import org.example.server.Context;

import java.nio.ByteBuffer;
import java.nio.channels.AsynchronousSocketChannel;
import java.util.concurrent.Future;

public class QueryMessage implements PGClientMessage {
    private String query;

    public QueryMessage(){

    }

    private QueryMessage(String query){

        this.query = query;
    }

    public void handleQueryMessage(AsynchronousSocketChannel client) {

    }

    public String getQuery() {
        return query;
    }

    @Override
    public PGClientMessage decode(ByteBuffer buffer) {
        var prev= buffer.position();
        var length = buffer.getInt(1);
        buffer.position(5);
        var query = PGClientMessage.extractStrings(buffer);
        buffer.position(prev+length);
        return new QueryMessage(query.get(0));
    }

    @Override
    public boolean isMatching(ByteBuffer b) {
        var pos = b.position();
        if(b.limit()<pos+1)return false;
        return b.get(pos+0)=='Q';
    }

    @Override
    public void handle(Context client) {
        //System.out.println("[SERVER] Query Message: " + this);

        Future<Integer> writeResult;

        // Let's assume it's a query message, and just send a simple response
        // First we send a RowDescription. We'll send two columns, with names "id" and "name"
        if(PgWireFakeServer.isUseFakeResponse()) {
            writeResult = FakeResultBuilder.buildFakeResult(client);
        }else{
            writeResult = RealResultBuilder.buildRealResult(this,client);
        }

        // Finally, write ReadyForQuery
        ReadyForQuery readyForQuery = new ReadyForQuery();
        writeResult = client.write(readyForQuery);

        try {
            writeResult.get();
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
