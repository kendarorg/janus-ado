package org.example.messages.extendedquery;

import org.example.messages.PGClientMessage;
import org.example.server.Context;

import java.nio.ByteBuffer;
import java.util.ArrayList;
import java.util.concurrent.ExecutionException;
import java.util.concurrent.Future;

public class BindMessage implements PGClientMessage {
    public String getDestinationPortalName() {
        return destinationPortalName;
    }

    public String getSourcePsName() {
        return sourcePsName;
    }

    public short[] getParamFormatCodes() {
        return paramFormatCodes;
    }

    public ArrayList<Object> getParameterValues() {
        return parameterValues;
    }

    public short[] getResultColumnFormatCodes() {
        return resultColumnFormatCodes;
    }

    private String destinationPortalName;
    private String sourcePsName;
    private short[] paramFormatCodes;
    private ArrayList<Object> parameterValues;
    private short[] resultColumnFormatCodes;

    public BindMessage(){

    }
    public BindMessage(String destinationPortalName, String sourcePsName, short[] paramFormatCodes, ArrayList<Object> parameterValues, short[] resultColumnFormatCodes) {

        this.destinationPortalName = destinationPortalName;
        this.sourcePsName = sourcePsName;
        this.paramFormatCodes = paramFormatCodes;
        this.parameterValues = parameterValues;
        this.resultColumnFormatCodes = resultColumnFormatCodes;
    }

    @Override
    public PGClientMessage decode(ByteBuffer buffer) {
        var prev= buffer.position();
        var length = buffer.getInt(prev+1);
        buffer.position(prev+1+4);
        var destinationPortalName = PGClientMessage.extractString(buffer);
        var sourcePsName = PGClientMessage.extractString(buffer);
        var paramFormatCodesCount = buffer.getShort();
        var paramFormatCodes = new short[paramFormatCodesCount];
        for(var i=0;i<paramFormatCodesCount;i++){
            paramFormatCodes[i]=buffer.getShort();
        }
        var parameterValuesThatFollow = buffer.getShort();
        var parameterValues = new ArrayList<Object>();
        if(parameterValuesThatFollow>0){
            for(var i=0;i<parameterValuesThatFollow;i++) {
                var parameterLength = buffer.getInt();
                var dst = new byte[parameterLength];

                for (var j=0; j < parameterLength; j++) {
                    dst[j] = buffer.get();
                }
                if(paramFormatCodes[i]==0) {
                    parameterValues.add(new String(dst));
                }else{
                    parameterValues.add(dst);
                }
            }
        }
        var resultColumnFormatCodesCount = buffer.getShort();
        var resultColumnFormatCodes = new short[resultColumnFormatCodesCount];
        for(var i=0;i<resultColumnFormatCodesCount;i++){
            resultColumnFormatCodes[i]=buffer.getShort();
        }
        buffer.position(prev+length+1);
        return new BindMessage(destinationPortalName,sourcePsName,paramFormatCodes,parameterValues,resultColumnFormatCodes);
    }

    @Override
    public boolean isMatching(ByteBuffer b) {
        var pos = b.position();
        return b.get(pos+0)=='B';
    }

    @Override
    public void handle(Context client, Future<Integer> prev) {
        if(prev!=null) {
            try {
                prev.get();
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
    }
}
