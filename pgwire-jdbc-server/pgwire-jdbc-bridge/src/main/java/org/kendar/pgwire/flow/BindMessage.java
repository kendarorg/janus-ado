package org.kendar.pgwire.flow;

import org.kendar.pgwire.commons.Context;
import org.kendar.pgwire.commons.DataMessage;
import org.kendar.pgwire.server.BindComplete;

import java.io.IOException;
import java.util.ArrayList;

public class BindMessage implements PgwFlowMessage{
    private String portal;
    private String statementName;
    private ArrayList<Short> formatCodes;
    private ArrayList<Object> parameterValues;

    @Override
    public void read(DataMessage message) throws IOException {
        portal = message.readString();
        statementName = message.readString();
        var formatCodesCount = message.getShort();
        formatCodes = new ArrayList<Short>();
        for(var i=0;i<formatCodesCount;i++){
            formatCodes.add(message.getShort());
        }
        var parameterValuesCount = message.getShort();
        parameterValues = new ArrayList<Object>();
        for(var i=0;i<parameterValuesCount;i++){
            var parameterLength = message.readInt();
            if(formatCodes.get(i)==0) {
                parameterValues.add(message.readString());
            }else{
                parameterValues.add(message.readBytes(parameterLength));
            }
        }
    }

    public String getPortal() {
        return portal;
    }

    public void setPortal(String portal) {
        this.portal = portal;
    }

    public String getStatementName() {
        return statementName;
    }

    public void setStatementName(String statementName) {
        this.statementName = statementName;
    }

    public ArrayList<Short> getFormatCodes() {
        return formatCodes;
    }

    public void setFormatCodes(ArrayList<Short> formatCodes) {
        this.formatCodes = formatCodes;
    }

    public ArrayList<Object> getParameterValues() {
        return parameterValues;
    }

    public void setParameterValues(ArrayList<Object> parameterValues) {
        this.parameterValues = parameterValues;
    }

    @Override
    public void handle(Context context) throws IOException {
        context.put("bind_portal_"+statementName,portal);
        context.put("bind_statement_"+portal,statementName);
        context.put("bind_"+statementName+"_"+portal,this);
        context.getBuffer().write(new BindComplete());
    }
}
