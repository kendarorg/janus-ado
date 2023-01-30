package org.example.messages.extendedquery;

import org.example.messages.PGClientMessage;
import org.example.server.Context;

import java.nio.ByteBuffer;
import java.util.ArrayList;

public class BindMessage implements PGClientMessage {
    private String destinationPortalName;
    private String sourcePsName;
    private short[] paramFormatCodes;
    private ArrayList<byte[]> parameterValues;
    private short[] resultColumnFormatCodes;

    public BindMessage(){

    }
    public BindMessage(String destinationPortalName, String sourcePsName, short[] paramFormatCodes, ArrayList<byte[]> parameterValues, short[] resultColumnFormatCodes) {

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
        var parameterValues = new ArrayList<byte[]>();
        if(parameterValuesThatFollow>0){
            var parameterLength = buffer.getInt();
            var dst = new byte[parameterLength];
            for(var i=0;i<parameterLength;i++){
                dst[i]=buffer.get();
            }
            parameterValues.add(dst);
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
    public void handle(Context client) {
    }
}
