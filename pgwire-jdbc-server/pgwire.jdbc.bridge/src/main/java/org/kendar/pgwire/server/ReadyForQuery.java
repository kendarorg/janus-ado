package org.kendar.pgwire.server;

import org.kendar.pgwire.commons.PgwByteBuffer;

import java.io.IOException;

public class ReadyForQuery implements PgwServerMessage{
    private final char transactionStatus;

    //public ReadyForQuery(){
    //    transactionStatus = 'I';
    //}

    public ReadyForQuery(boolean inTransaction){
        transactionStatus = inTransaction?'T':'I';
    }
    @Override
    public void write(PgwByteBuffer buffer) throws IOException {
        buffer.writeByte((byte) 'Z'); // 'Z' for ReadyForQuery
        buffer.writeInt(5); // Length
        buffer.writeByte((byte) transactionStatus);
    }
}
