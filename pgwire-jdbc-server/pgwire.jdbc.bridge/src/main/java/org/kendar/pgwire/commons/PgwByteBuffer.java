package org.kendar.pgwire.commons;

import org.kendar.pgwire.initialize.PgwClientMessage;
import org.kendar.pgwire.server.PgwServerMessage;

import java.io.*;
import java.net.Socket;
import java.util.ArrayList;
import java.util.List;

public class PgwByteBuffer {
    private DataInputStream inputStream;
    private DataOutputStream outputStream;
    private Socket socket;

    public PgwByteBuffer(Socket socket) {
        this.socket = socket;

        try {
            this.inputStream = new DataInputStream(new BufferedInputStream(socket.getInputStream()));
            this.outputStream = new DataOutputStream(new BufferedOutputStream(socket.getOutputStream()));
        } catch (IOException e) {
            throw new RuntimeException(e);
        }
    }

    public byte[] read(int size) throws IOException {
        var tmpData = new byte[size];
        var redBytes = 0;
        while(redBytes<size) {
            var partial = this.inputStream.read(tmpData, redBytes, size-redBytes);
            if(partial==0 && redBytes!=size)throw new IOException("Missing data");
            redBytes+=partial;
        }
        return tmpData;
    }

    public byte readByte() throws IOException{
        return inputStream.readByte();
    }

    public int readInt32() throws IOException {
        return inputStream.readInt();
        /*byte[] bytes=new byte[4];
        inputStream.read(bytes);
        return 112;/*((bytes[1] & 0xFF) << 24) | ((bytes[0] & 0xFF) << 16)
                | ((bytes[3] & 0xFF) << 8) | (bytes[2] & 0xFF);*/
    }

    public void read(PgwClientMessage pgwClientMessage) throws IOException {
        System.out.println("[SERVER] BEG Recv: "+pgwClientMessage.getClass().getSimpleName());
        pgwClientMessage.read(this);
        System.out.println("[SERVER] END Recv: "+pgwClientMessage.getClass().getSimpleName());
    }

    public PgwByteBuffer write(PgwServerMessage pgwServerMessage) throws IOException {
        System.out.println("[SERVER] Sent: "+pgwServerMessage.getClass().getSimpleName());
        pgwServerMessage.write(this);
        outputStream.flush();
        return this;
    }

    public PgwByteBuffer writeByte(byte n) throws IOException {
        outputStream.writeByte(n);
        return this;
    }

    public PgwByteBuffer writeInt(int i) throws IOException {
        outputStream.writeInt(i);
        return this;
    }

    public PgwByteBuffer writeShort(short s) throws IOException {
        outputStream.writeShort(s);
        return this;
    }

    public List<String> readStrings(int bufferLength) throws IOException {
        var result = new ArrayList<String>();
        var data = read(bufferLength);
        var start = 0;
        var count =0;
        for(var i=0;i<data.length;i++){
            count++;
            if(data[i]==0x00){
                result.add(new String(data,start,count));
                i++;
                start=i;
                count = 0;
            }
        }

        return result;
    }

    public PgwByteBuffer write(byte[] data) throws IOException {
        outputStream.write(data);
        return this;
    }
}
