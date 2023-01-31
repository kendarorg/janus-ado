package org.example.messages;

import org.example.builders.FakeResultBuilder;
import org.example.PgWireFakeServer;
import org.example.builders.RealResultBuilder;
import org.example.messages.extendedquery.ParseMessage;
import org.example.server.Context;

import java.nio.ByteBuffer;
import java.util.concurrent.Future;

public class ExecuteMessage implements PGClientMessage {

    private String portal;
    private int maxRecords;

    public ExecuteMessage(String portal, int maxRecords) {

        this.portal = portal;
        this.maxRecords = maxRecords;
    }

    public ExecuteMessage() {

    }

    @Override
    public PGClientMessage decode(ByteBuffer buffer) {
        var prev = buffer.position();
        buffer.position(prev+1);
        var length = buffer.getInt();
        //buffer.position(5);
        var portal = PGClientMessage.extractString(buffer);
        var maxRecords = buffer.getInt();
        return new ExecuteMessage(portal, maxRecords);
    }

    @Override
    public boolean isMatching(ByteBuffer b) {
        var pos = b.position();
        if (b.limit() < pos + 1) return false;
        return b.get(pos + 0) == 'E';
    }

    @Override
    public void handle(Context client) {
        Future<Integer> writeResult = null;
        if (PgWireFakeServer.isUseFakeResponse()) {
            writeResult = FakeResultBuilder.buildFakeResult(client);
        } else {
            var msg = client.get((o) -> {
                if (o instanceof ParseMessage) {
                    if (((ParseMessage) o).getPsName().equalsIgnoreCase(portal)) {
                        return true;
                    }
                }
                return false;
            });
            writeResult = RealResultBuilder.buildRealResult((ParseMessage) msg,client);
        }


        try {
            writeResult.get();
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
