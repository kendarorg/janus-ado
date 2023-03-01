package org.kendar.pgwire.server;

import org.kendar.pgwire.commons.PgwByteBuffer;
import org.kendar.pgwire.utils.Field;

import java.io.IOException;
import java.nio.ByteBuffer;
import java.util.List;

public class DataRow  implements PgwServerMessage{
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
    public void write(PgwByteBuffer buffer) throws IOException {
        var length = 4 + 2 + values.stream().map(it -> it.remaining() + 4).reduce(0, Integer::sum);
        buffer // +1 for msg type
                .writeByte((byte) 'D')
                .writeInt(length) // +4 for length
                .writeShort((short) values.size()); // +2 for number of columns
        for (var value : values) {
            buffer.writeInt(value.remaining());
            buffer.write(value.array());
        }
    }
}