package org.example.messages;

import org.example.messages.sslnegotiation.SSLResponse;
import org.example.server.Context;

import java.nio.ByteBuffer;

public class SSLNegotation implements PGClientMessage {
    public SSLNegotation(){

    }

    @Override
    public PGClientMessage decode(ByteBuffer source) {
        var pos = source.position();
        source.position(pos+8);
        return new SSLNegotation();
    }

    @Override
    public boolean isMatching(ByteBuffer b) {
        var pos = b.position();
        if(b.limit()<pos+7)return false;
        return b.get(pos+4) == 0x04
                && b.get(pos+5) == (byte) 0xd2
                && b.get(pos+6) == 0x16
                && b.get(pos+7) == 0x2f;
    }

    @Override
    public void handle(Context client) {
        //System.out.println("[SERVER] SSL Request: " + this);

        try {
            client.write(new SSLResponse()).get();
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
