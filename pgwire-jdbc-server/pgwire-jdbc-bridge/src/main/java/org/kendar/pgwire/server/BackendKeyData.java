package org.kendar.pgwire.server;

import org.kendar.pgwire.commons.PgwByteBuffer;

import java.io.IOException;

public class BackendKeyData implements PgwServerMessage{
    private int processId;
    private int secret;

    public int getProcessId() {
        return processId;
    }

    public void setProcessId(int processId) {
        this.processId = processId;
    }

    public int getSecret() {
        return secret;
    }

    public void setSecret(int secret) {
        this.secret = secret;
    }

    public BackendKeyData(int processId, int secret) {
        this.processId = processId;
        this.secret = secret;
    }

    @Override
    public void write(PgwByteBuffer buffer) throws IOException {
        buffer.writeByte((byte) 'K'); // 'K' for BackendKeyData
        buffer.writeInt(12); // Length
        buffer.writeInt(this.getProcessId()); // Process ID
        buffer.writeInt(this.getSecret()); // Secret key
    }
}
