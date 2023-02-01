package org.example.messages.querymessage;

import org.example.messages.PGServerMessage;

import java.nio.ByteBuffer;
import java.util.ArrayList;
import java.util.List;

public class DataRow implements PGServerMessage {
    private List<ByteBuffer> values;
    private List<Field> fields;

    public DataRow(List<ByteBuffer> values, List<Field> fields){

        this.values = values;
        this.fields = fields;
    }

    public List<ByteBuffer> getValues() {
        return values;
    }

    @Override
    public ByteBuffer encode() {
        var values = this.getValues();
        // For each value, we need to add 4 bytes for the length, plus the length of the value
        var length = 4 + 2 + values.stream().map(it -> it.remaining() + 4).reduce(0, Integer::sum);
        var buffer = ByteBuffer.allocate(length + 1) // +1 for msg type
                .put((byte) 'D')
                .putInt(length) // +4 for length
                .putShort((short) values.size()); // +2 for number of columns
        for (var value : values) {
            buffer.putInt(value.remaining());
            buffer.put(value);
        }
        buffer.flip();
        return buffer;
    }
}
