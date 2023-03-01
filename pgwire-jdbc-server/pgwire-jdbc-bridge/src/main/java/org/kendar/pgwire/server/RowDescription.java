package org.kendar.pgwire.server;

import org.kendar.pgwire.commons.PgwByteBuffer;
import org.kendar.pgwire.utils.Field;

import java.io.IOException;
import java.nio.ByteBuffer;
import java.nio.charset.StandardCharsets;
import java.util.ArrayList;

public class RowDescription implements PgwServerMessage{
    private ArrayList<Field> fields;

    public RowDescription(ArrayList<Field> fields) {

        this.fields = fields;
    }

    @Override
    public void write(PgwByteBuffer buffer) throws IOException {
        var length = 4 + 2 + fields.stream().mapToInt(Field::length).sum();
        buffer
                .writeByte((byte) 'T')
                .writeInt(length)
                .writeShort((short) fields.size());

        for (Field field : fields) {
            buffer
                    .write(field.getName().getBytes(StandardCharsets.UTF_8))
                    .writeByte((byte) 0) // null-terminated
                    .writeInt(field.getTableObjectId())
                    .writeShort((short) field.getColumnAttributeNumber())
                    .writeInt(field.getDataTypeObjectId())
                    .writeShort((short) field.getDataTypeSize())
                    .writeInt(field.getTypeModifier())
                    .writeShort((short) field.getFormatCode());
        }
    }
}
