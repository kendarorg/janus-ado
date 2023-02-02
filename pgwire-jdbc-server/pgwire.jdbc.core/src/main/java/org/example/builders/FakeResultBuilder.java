package org.example.builders;

import org.example.messages.querymessage.CommandComplete;
import org.example.messages.querymessage.DataRow;
import org.example.messages.querymessage.Field;
import org.example.messages.querymessage.RowDescription;
import org.example.server.Context;
import org.example.server.TypesOids;

import java.nio.ByteBuffer;
import java.nio.charset.StandardCharsets;
import java.util.List;
import java.util.concurrent.Future;

public class FakeResultBuilder {
    public static Future<Integer> buildFakeResult(Context client){

        Future<Integer> writeResult=null;
        // Let's assume it's a query message, and just send a simple response
        // First we send a RowDescription. We'll send two columns, with names "id" and "name"
        RowDescription rowDescription = new RowDescription(List.of(
                //new Field("id", 0, 0, TypesOids.Int4, 4, -1, 0,),
                //new Field("name", 0, 0, TypesOids.Text, -1, -1, 0)
        ));
        writeResult = client.write(rowDescription);


        // Then we send a DataRow for each row. We'll send two rows, with values (1, "one") and (2, "two")
        DataRow dataRow1 = new DataRow(List.of(
                ByteBuffer.wrap("1".getBytes(StandardCharsets.UTF_8)),
                ByteBuffer.wrap("one".getBytes(StandardCharsets.UTF_8))
        ), rowDescription.getFields());
        writeResult = client.write(dataRow1);

        DataRow dataRow2 = new DataRow(List.of(
                ByteBuffer.wrap("2".getBytes(StandardCharsets.UTF_8)),
                ByteBuffer.wrap("two".getBytes(StandardCharsets.UTF_8))
        ), rowDescription.getFields());

        writeResult = client.write(dataRow2);

        // We send a CommandComplete
        CommandComplete commandComplete = new CommandComplete("SELECT 2");
        writeResult = client.write(commandComplete);

        return writeResult;
    }
}
