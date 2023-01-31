package org.example.messages;

import org.example.server.Context;

import java.nio.ByteBuffer;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;
import java.util.List;

public interface PGClientMessage {
    PGClientMessage decode(ByteBuffer buffer);
    boolean isMatching(ByteBuffer b);
    void handle(Context client);


    static List<String> extractStrings(ByteBuffer buffer) {
        var strings = new ArrayList<String>();
        int i;
        while (buffer.hasRemaining()) {
            ByteBuffer nextString = buffer.slice(); // View on b with same start position
            for (i = 0; buffer.hasRemaining() && buffer.get() != 0x00; i++) {
                // Count to next NUL
            }
            nextString.limit(i); // view now stops before NUL
            strings.add(String.valueOf(StandardCharsets.UTF_8.decode(nextString)));
        }
        return strings;
    }
    static String extractString(ByteBuffer buffer) {

        int i;
        while (buffer.hasRemaining()) {
            ByteBuffer nextString = buffer.slice(); // View on b with same start position
            for (i = 0; buffer.hasRemaining() && buffer.get() != 0x00; i++) {
                // Count to next NUL
            }
            nextString.limit(i); // view now stops before NUL
            return String.valueOf(StandardCharsets.UTF_8.decode(nextString));
        }
        throw new RuntimeException("WTF");
    }

}
