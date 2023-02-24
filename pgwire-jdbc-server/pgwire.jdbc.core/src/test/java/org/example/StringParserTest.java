package org.example;

import org.junit.jupiter.api.Test;

import static org.junit.jupiter.api.Assertions.assertEquals;

public class StringParserTest {

    @Test
    public void quotesHandling(){
        var result = StringParser.parseString("Example 'te''st' simple");
        assertEquals(3,result.size());
        assertEquals("Example ",result.get(0));
        assertEquals("'te''st'",result.get(1));
        assertEquals(" simple",result.get(02));

        result = StringParser.parseString("Example 'test' simple");
        assertEquals(3,result.size());
        assertEquals("Example ",result.get(0));
        assertEquals("'test'",result.get(1));
        assertEquals(" simple",result.get(02));

        result = StringParser.parseString("Example 'te\\'st' simple");
        assertEquals(3,result.size());
        assertEquals("Example ",result.get(0));
        assertEquals("'te\\'st'",result.get(1));
        assertEquals(" simple",result.get(02));

        result = StringParser.parseString("Example \"test\" simple");
        assertEquals(3,result.size());
        assertEquals("Example ",result.get(0));
        assertEquals("\"test\"",result.get(1));
        assertEquals(" simple",result.get(02));

        result = StringParser.parseString("Example \"te\\\"st\" simple");
        assertEquals(3,result.size());
        assertEquals("Example ",result.get(0));
        assertEquals("\"te\\\"st\"",result.get(1));
        assertEquals(" simple",result.get(02));
    }

    @Test
    public void semicolumnHandling(){
        var result = StringParser.parseSql("SELECT 1;UPDATE test SET name='y''o' WHERE 1=0;");
        assertEquals(2,result.size());
        assertEquals("SELECT 1;",result.get(0));
        assertEquals("UPDATE test SET name='y''o' WHERE 1=0;",result.get(1));

        result = StringParser.parseSql("SELECT 1;UPDATE test SET name='y''o' WHERE 1=0");
        assertEquals(2,result.size());
        assertEquals("SELECT 1;",result.get(0));
        assertEquals("UPDATE test SET name='y''o' WHERE 1=0",result.get(1));
    }
}
