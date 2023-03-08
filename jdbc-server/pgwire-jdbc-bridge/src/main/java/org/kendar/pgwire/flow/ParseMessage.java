package org.kendar.pgwire.flow;

import org.kendar.pgwire.commons.Context;
import org.kendar.pgwire.commons.DataMessage;
import org.kendar.pgwire.server.ParseCompleted;
import org.kendar.pgwire.server.ReadyForQuery;

import java.io.IOException;
import java.util.ArrayList;
import java.util.Locale;
import java.util.Set;
import java.util.concurrent.ConcurrentSkipListSet;

public class ParseMessage implements PgwFlowMessage{
    private String statementName;
    private String query;

    public String getStatementName() {
        return statementName;
    }

    public void setStatementName(String statementName) {
        this.statementName = statementName;
    }

    public String getQuery() {
        return query;
    }

    public void setQuery(String query) {
        this.query = query;
    }

    public ArrayList<Integer> getOids() {
        return oids;
    }

    public void setOids(ArrayList<Integer> oids) {
        this.oids = oids;
    }

    private ArrayList<Integer> oids = new ArrayList<>();



    @Override
    public void read(DataMessage message) throws IOException {
        statementName = message.readString();
        query = message.readStringUtf8();
        System.out.println("[SERVER] ExtQuery: "+query);
        var paramsCount = message.getShort();
        oids = new ArrayList<Integer>();
        for(var i=0;i<paramsCount;i++){
            oids.add(message.readInt());
        }
    }

    @Override
    public void handle(Context context) throws IOException {
        context.put("statement_"+statementName,this);

        context.getBuffer().write(new ParseCompleted());
    }
}
