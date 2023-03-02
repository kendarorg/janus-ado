package org.kendar.pgwire.utils;

import java.time.LocalDateTime;
import java.time.format.DateTimeFormatter;

public class ConsoleOut {
    public static void println(String val){
        var now = LocalDateTime.now().format(DateTimeFormatter.ofPattern("yyyy-MM-dd HH:mm:ss.SSS"));
        System.out.println(now+" "+val);
    }
}
