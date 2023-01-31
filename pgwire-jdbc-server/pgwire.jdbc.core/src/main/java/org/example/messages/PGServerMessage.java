package org.example.messages;

import java.nio.ByteBuffer;
import java.nio.charset.StandardCharsets;

public interface PGServerMessage {
    ByteBuffer encode();
}
