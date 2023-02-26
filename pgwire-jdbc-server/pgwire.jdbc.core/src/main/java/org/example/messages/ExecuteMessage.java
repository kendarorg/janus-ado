package org.example.messages;

import org.example.builders.RealResultBuilder;
import org.example.messages.extendedquery.ParseMessage;
import org.example.server.Context;

import java.nio.ByteBuffer;
import java.util.concurrent.Future;

public class ExecuteMessage implements PGClientMessage {

    public String getPortal() {
        return portal;
    }

    public void setPortal(String portal) {
        this.portal = portal;
    }

    public void setMaxRecords(int maxRecords) {
        this.maxRecords = maxRecords;
    }

    private String portal;
    private int maxRecords;
    private String psName;

    public ExecuteMessage(String portal, int maxRecords) {

        this.portal = portal;
        this.maxRecords = maxRecords;
    }

    public ExecuteMessage(String psName, String sourcePortal) {
        this.psName = psName;
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
    public void handle(Context client, Future<Integer> prev) {


        try {
            Future<Integer> writeResult = null;
            var msg = client.get(psName+"_"+portal);
            writeResult = RealResultBuilder.buildRealResultPs((ParseMessage) msg,client,prev,psName, portal,maxRecords);

            writeResult.get();
        } catch (Exception e) {
            e.printStackTrace();
        }
    }

    public int getMaxRecords() {
        return maxRecords;
    }

    public String getPsName() {
        return psName;
    }

    public void setPsName(String psName) {
        this.psName = psName;
    }
}
