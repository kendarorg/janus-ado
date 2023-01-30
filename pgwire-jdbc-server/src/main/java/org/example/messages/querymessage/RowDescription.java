package org.example.messages.querymessage;

import org.example.messages.PGServerMessage;

import java.nio.ByteBuffer;
import java.nio.charset.StandardCharsets;
import java.util.List;

public class RowDescription implements PGServerMessage {
    private List<Field> fields;

    public RowDescription(List<Field> fields){

        this.fields = fields;
    }

    public List<Field> getFields() {
        return fields;
    }

    @Override
    public ByteBuffer encode() {
        var fields = this.getFields();
        var length = 4 + 2 + fields.stream().mapToInt(Field::length).sum();
        var buffer = ByteBuffer.allocate(length + 1)
                .put((byte) 'T')
                .putInt(length)
                .putShort((short) fields.size());
        var finalBuffer=buffer;
        fields.forEach(field -> finalBuffer
                .put(field.getName().getBytes(StandardCharsets.UTF_8))
                .put((byte) 0) // null-terminated
                .putInt(field.getTableObjectId())
                .putShort((short) field.getColumnAttributeNumber())
                .putInt(field.getDataTypeObjectId())
                .putShort((short) field.getDataTypeSize())
                .putInt(field.getTypeModifier())
                .putShort((short) field.getFormatCode()));
        buffer.flip();
        return buffer;
    }
}
