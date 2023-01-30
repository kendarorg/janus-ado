package org.example.messages.startupmessage;

import org.example.messages.PGServerMessage;

import java.nio.ByteBuffer;

public class BackendKeyData implements PGServerMessage {
    public int getProcessId() {
        return processId;
    }

    public int getSecretKey() {
        return secretKey;
    }

    private final int processId;
    private final int secretKey;

    public BackendKeyData(int processId, int secretKey)
    {

        this.processId = processId;
        this.secretKey = secretKey;
    }

    @Override
    public ByteBuffer encode() {
        var buffer = ByteBuffer.allocate(13);
        buffer.put((byte) 'K'); // 'K' for BackendKeyData
        buffer.putInt(12); // Length
        buffer.putInt(this.getProcessId()); // Process ID
        buffer.putInt(this.getSecretKey()); // Secret key
        buffer.flip();
        return buffer;
    }
}
