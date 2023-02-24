package org.example.messages.querymessage;

import org.example.messages.PGServerMessage;

import java.nio.ByteBuffer;
import java.nio.charset.StandardCharsets;

public class CommandComplete implements PGServerMessage {
    @Override
    public String toString() {
        return "CommandComplete{" +
                "commandTag='" + commandTag + '\'' +
                '}';
    }

    public String getCommandTag() {
        return commandTag;
    }

    private String commandTag;

    public CommandComplete(String commandTag){

        this.commandTag = commandTag;
    }

    @Override
    public ByteBuffer encode() {
        var commandTag = this.getCommandTag();
        var length = 4 + commandTag.length() + 1;
        var buffer = ByteBuffer.allocate(length + 1) // +1 for msg type
                .put((byte) 'C')
                .putInt(length) // +4 for length
                .put(commandTag.getBytes(StandardCharsets.UTF_8))
                .put((byte) 0) // null terminator
                .flip();
        return buffer;
    }
}
