package org.kendar.pgwire.initialize;

import org.kendar.pgwire.commons.Context;
import org.kendar.pgwire.commons.PgwByteBuffer;
import org.kendar.pgwire.server.AuthenticationOk;
import org.kendar.pgwire.server.BackendKeyData;
import org.kendar.pgwire.server.ParameterStatus;
import org.kendar.pgwire.server.ReadyForQuery;

import java.io.IOException;
import java.util.HashMap;
import java.util.Map;

public class StartupMessage implements PgwClientMessage{
    private final int length;
    private Context context;
    private Map<String, String> parameters;
    private int pid;

    public StartupMessage(int pid, int firstLength, Context context) {
        this.pid = pid;
        this.length = firstLength;
        this.context = context;
    }

    @Override
    public void read(PgwByteBuffer buffer) throws IOException {
        var startupMessageRemainingLength = length; //112-8
        if(length==-1){
            startupMessageRemainingLength = buffer.readInt32();
        }
        var protocolVersion = buffer.readInt32(); //196608
        var flatMap = buffer.readStrings(startupMessageRemainingLength-8);
        parameters = new HashMap<String, String>();
        for(var j=0;j<flatMap.size()-1;j+=2){
            parameters.put(flatMap.get(j),flatMap.get(j+1));
        }
        if(parameters.containsKey("janus")){
            context.setJanus(true);
        }
        buffer.write(new AuthenticationOk());
        buffer.write(new ParameterStatus("server_version","15"));
        buffer.write(new ParameterStatus("server_type","JANUS"));
        buffer.write(new ParameterStatus("client_encoding","UTF8"));
        buffer.write(new ParameterStatus("DateStyle","ISO, MDY"));
        buffer.write(new ParameterStatus("TimeZone","CET"));
        buffer.write(new ParameterStatus("is_superuser","on"));
        buffer.write(new ParameterStatus("integer_datetimes","on"));
        buffer.write(new BackendKeyData(pid, 5678));
        buffer.write(new ReadyForQuery(false));
    }
}
