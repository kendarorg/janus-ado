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

    @Test
    public void standardQuery(){
        var result = StringParser.getTypes("SELECT 1;UPDATE test SET name='y''o' WHERE 1=0;");
        assertEquals(2,result.size());
        assertEquals("SELECT 1;",result.get(0).getValue());
        assertEquals(SqlStringType.SELECT,result.get(0).getType());
        assertEquals("UPDATE test SET name='y''o' WHERE 1=0;",result.get(1).getValue());
        assertEquals(SqlStringType.UPDATE,result.get(1).getType());
    }

    @Test
    public void withError(){
        var result = StringParser.getTypes("\r\nDROP TABLE IF EXISTS temp_table1 CASCADE;\r\n CREATE TABLE temp_table1 (intf int);\r\n");
        assertEquals(2,result.size());
        assertEquals("\r\nDROP TABLE IF EXISTS temp_table1 CASCADE;",result.get(0).getValue());
        assertEquals(SqlStringType.UPDATE,result.get(0).getType());
        assertEquals("\r\n CREATE TABLE temp_table1 (intf int);",result.get(1).getValue());
        assertEquals(SqlStringType.UPDATE,result.get(1).getType());
    }

}
