package org.kendar.pgwire.initialize;

import org.kendar.pgwire.commons.PgwByteBuffer;
import org.kendar.pgwire.server.AuthenticationOk;
import org.kendar.pgwire.server.BackendKeyData;
import org.kendar.pgwire.server.ParameterStatus;
import org.kendar.pgwire.server.ReadyForQuery;

import java.io.IOException;
import java.util.HashMap;
import java.util.Map;

public class StartupMessage implements PgwClientMessage{
    private Map<String, String> parameters;
    private int pid;

    public StartupMessage(int pid) {
        this.pid = pid;
    }

    @Override
    public void read(PgwByteBuffer buffer) throws IOException {
        var startupMessageRemainingLength = buffer.readInt32(); //112-8
        var protocolVersion = buffer.readInt32(); //196608
        var flatMap = buffer.readStrings(startupMessageRemainingLength-8);
        parameters = new HashMap<String, String>();
        for(var j=0;j<flatMap.size()-1;j+=2){
            parameters.put(flatMap.get(j),flatMap.get(j+1));
        }

        buffer.write(new AuthenticationOk());
        buffer.write(new BackendKeyData(pid, 5678));
        buffer.write(new ParameterStatus("server_version","15"));
        buffer.write(new ParameterStatus("server_type","JANUS"));
        buffer.write(new ReadyForQuery(false));
    }
}
