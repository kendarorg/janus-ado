package org.example.messages;

import org.example.messages.startupmessage.AuthenticationOk;
import org.example.messages.startupmessage.BackendKeyData;
import org.example.messages.commons.ReadyForQuery;
import org.example.messages.startupmessage.ParameterStatus;
import org.example.server.Context;

import java.nio.ByteBuffer;
import java.util.HashMap;
import java.util.Map;
import java.util.concurrent.Future;

public class StartupMessage implements PGClientMessage {
    private Map<String, String> parameters;

    public StartupMessage(){

    }

    protected StartupMessage(Map<String, String> parameters){

        this.parameters = parameters;
    }

    public Map<String, String> getParameters() {
        return parameters;
    }


    @Override
    public PGClientMessage decode(ByteBuffer buffer) {
        var prev= buffer.position();

        var length = buffer.getInt(prev+0);
        var protocolVersion = buffer.getInt(prev+4);
        buffer.position(prev+8);
        int i;

        var strings = PGClientMessage.extractStrings(buffer);
        var parameters = new HashMap<String, String>();
        for(var j=0;j<strings.size()-1;j+=2){
            parameters.put(strings.get(j),strings.get(j+1));
        }
        //buffer.position(prev + length);
        return new StartupMessage(parameters);
    }

    @Override
    public boolean isMatching(ByteBuffer b) {
        var pos = b.position();
        if(b.limit()<pos+7)return false;
        return b.remaining() > 8
                && b.get(pos+4) == 0x00
                && b.get(pos+5) == 0x03 // Protocol version 3
                && b.get(pos+6) == 0x00
                && b.get(pos+7) == 0x00;
    }

    @Override
    public void handle(Context client, Future<Integer> prev) {
        //System.out.println("[SERVER] Startup Message: " + this);

        Future<Integer> writeResult;

        // Then, write AuthenticationOk
        AuthenticationOk authRequest = new AuthenticationOk();
        writeResult = client.write(authRequest,prev);


        // Then, write BackendKeyData
        BackendKeyData backendKeyData = new BackendKeyData(1234, 5678);
        writeResult = client.write(backendKeyData,writeResult);

        ParameterStatus serverVersion = new ParameterStatus("server_version","15");
        writeResult = client.write(serverVersion,writeResult);

        // Then, write ReadyForQuery
        ReadyForQuery readyForQuery = new ReadyForQuery();
        writeResult = client.write(readyForQuery,writeResult);

        try {
            writeResult.get();
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
