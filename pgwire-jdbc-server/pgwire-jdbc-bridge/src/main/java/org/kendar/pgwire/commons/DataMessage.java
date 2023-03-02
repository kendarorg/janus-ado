package org.kendar.pgwire.commons;

import java.nio.ByteBuffer;

public class DataMessage {
    private int cursor = 0;
    private char type;
    private int length;
    private byte[] data;

    public char getType() {
        return type;
    }

    public void setType(char type) {
        this.type = type;
    }

    public int getLength() {
        return length;
    }

    public void setLength(int length) {
        this.length = length;
    }

    public byte[] getData() {
        return data;
    }

    public void setData(byte[] data) {
        this.data = data;
    }

    public DataMessage(char type, int length, byte[] data) {
        this.type = type;
        this.length = length;
        this.data = data;
    }

    public int readInt(){
        var result = ByteBuffer.wrap(data,cursor,4).getInt();
        cursor+=4;
        return result;
    }

    public short getShort() {
        var result = ByteBuffer.wrap(data,cursor,2).getShort();
        cursor+=2;
        return result;
    }

    public String readString(){
        var base = cursor;
        var count =0;
        for(;cursor<data.length;cursor++){

            if(data[cursor]==0x00){
                cursor++;
                break;
            }
            count++;
        }
        return new String(data,base,count);
    }

    public byte[] readBytes(int parameterLength) {
        var result = new byte[parameterLength];
        for(var i=0;i<parameterLength && cursor<data.length;i++,cursor++){
            result[i]=data[cursor];
        }
        return result;
    }

    public byte readByte() {
        cursor++;
        return data[cursor-1];
    }
}
