package org.example.messages.extendedquery;

import org.example.messages.ExecuteMessage;
import org.example.messages.PGClientMessage;
import org.example.messages.SyncMessage;
import org.example.messages.commons.ReadyForQuery;
import org.example.messages.extendedquery.responses.BindCompleted;
import org.example.messages.extendedquery.responses.ParseCompleted;
import org.example.server.Context;

import java.nio.ByteBuffer;
import java.util.*;
import java.util.concurrent.ConcurrentSkipListSet;
import java.util.concurrent.Future;

public class ParseMessage implements PGClientMessage {
    private String psName;
    private int[] paramsOids;

    public String getPsName() {
        return psName;
    }

    public String getQuery() {
        return query;
    }

    public short getParamsCount() {
        return paramsCount;
    }

    public int[] getOid() {
        return oid;
    }



    public ArrayList<DescribeMessage> getDescribes() {
        return describes;
    }

    private String query;
    private short paramsCount;
    private int[] oid;
    private ByteBuffer buffer;
    private ArrayList<DescribeMessage> describes;

    public ParseMessage(){

    }

    public ParseMessage(String psName, String query, short paramsCount, int[] oid, ByteBuffer buffer) {

        this.psName = psName;
        this.query = query;
        this.paramsCount = paramsCount;
        this.oid = oid;
        this.buffer = buffer;
    }

    @Override
    public PGClientMessage decode(ByteBuffer buffer) {
        var prev = buffer.position();
        var length = buffer.getInt(prev+1);
        buffer.position(prev+1+4);
        var psName = PGClientMessage.extractString(buffer);
        //System.out.println("[SERVER] received PSNAME "+psName);
        var query = PGClientMessage.extractString(buffer);
        var paramsCount = buffer.getShort();
        var paramsOids = new int[paramsCount];
        for(var i=0;i<paramsCount;i++){
            paramsOids[i]=buffer.getInt();
        }

        return new ParseMessage(psName,query,paramsCount,paramsOids,buffer);
    }

    @Override
    public boolean isMatching(ByteBuffer b) {
        var pos = b.position();
        return b.get(pos+0)=='P';
    }

    private static Set<String> fakeQueries;

    static {
        fakeQueries = new ConcurrentSkipListSet<>();
        fakeQueries.add("SET extra_float_digits".toLowerCase(Locale.ROOT));
        fakeQueries.add("SET application_name".toLowerCase(Locale.ROOT));
    }

    @Override
    public void handle(Context client, Future<Integer> prev) {
        Future<Integer> writeResult=null;
        if(fakeQueries.stream().anyMatch(a->query.toLowerCase(Locale.ROOT).startsWith(a))){
            System.out.println("[SERVER] Fake: "+query);
            ReadyForQuery readyForQuery = new ReadyForQuery();
            writeResult = client.write(readyForQuery,prev);
            buffer.position(buffer.limit());
        }else {
            System.out.println("[SERVER] Parse: "+query);
            //client.add((PGClientMessage)this);

            var statementName = this.psName;
            var portal = "";

            client.put("statement_"+statementName,this);
            ParseCompleted parseCompleted = new ParseCompleted();
            writeResult = client.write(parseCompleted,prev);
            /*BindMessage bind = new BindMessage();
            if(bind.isMatching(buffer)) {
                System.out.println("[SERVER] ParseMessage-Received: BindMessage");
                binds=(BindMessage) bind.decode(buffer);
                portal = bind.getDestinationPortalName();
            }


            BindCompleted bindCompleted = new BindCompleted();
            writeResult = client.write(bindCompleted,writeResult);
            DescribeMessage describeMessage = new DescribeMessage();
            describes = new ArrayList<DescribeMessage>();
            while(describeMessage.isMatching(buffer)) {
                System.out.println("[SERVER] ParseMessage-Received: DescribeMessage");
                describes.add((DescribeMessage) describeMessage.decode(buffer));
            }*/
            /*ExecuteMessage executeMessage = new ExecuteMessage(psName,portal);
            if(executeMessage.isMatching(buffer)){
                System.out.println("[SERVER] ParseMessage-Received: ExecuteMessage");
                var message = (ExecuteMessage)executeMessage.decode(buffer);
                message.setPsName(statementName);
                message.handle(client,writeResult);
            }*/


            /*SyncMessage syncMessage = new SyncMessage();
            if(syncMessage.isMatching(buffer)){
                System.out.println("[SERVER] ParseMessage-Received: SyncMessage");
                var message = syncMessage.decode(buffer);
                message.handle(client,writeResult);
            }*/


            // Finally, write ReadyForQuery

        }


        try {
            writeResult.get();
        } catch (Exception e) {
            //e.printStackTrace();
        }
    }
}
