package org.example;

import java.util.ArrayList;
import java.util.List;

public class StringParser {

    /*public static void main(String[] args) {
        String input = "Hello, 'world!'";
        String[] tokens = parseString(input);
        for (String token : tokens) {
            System.out.println(token);
        }
    }*/

    public static List<String> parseSql(String input){
        List<String> sqls = new ArrayList<>();
        var splitted = parseString(input);

        String tempValue="";
        for(var line:splitted){
            var trimmed = line.strip();
            if(trimmed.startsWith("'")||trimmed.startsWith("\"")){
                //Is a string
                tempValue += line;
                continue;
            }
            if(trimmed.contains(";")){
                for(var part:line.split("((?<=;))")){
                    var trimPart = part.trim();
                    if(trimPart.endsWith(";")) {
                        tempValue += part;
                        sqls.add(tempValue);
                        tempValue="";
                    }else {
                        tempValue += part;
                    }
                }
                continue;
            }
            tempValue+=line;
        }
        if(tempValue.length()>0){
            sqls.add(tempValue);
        }
        return sqls;
    }
    public static List<String> parseString(String input) {
        List<String> tokens = new ArrayList<>();
        int length = input.length();
        int i = 0;
        while (i < length) {
            char c = input.charAt(i);
            if (c == '\'' || c == '\"') {
                StringBuilder sb = new StringBuilder();
                char delimiter = c;
                sb.append(c); // include starting quote
                i++; // skip delimiter
                while (i < length) {
                    c = input.charAt(i);
                    if (c == delimiter) {
                        if (i + 1 < length && input.charAt(i + 1) == delimiter && delimiter=='\'') {
                            // Handle doubled delimiter
                            sb.append(delimiter);
                            sb.append(delimiter);
                            i += 2;
                        } else {
                            // End of string
                            sb.append(c);
                            i++;
                            break;
                        }
                    } else if (c == '\\') {
                        // Handle escaped character
                        sb.append(c);
                        i++;
                        if (i < length) {
                            sb.append(input.charAt(i));
                            i++;
                        }
                    } else {
                        sb.append(c);
                        i++;
                    }
                }
                tokens.add(sb.toString());
            } else {
                // Handle non-string token
                StringBuilder sb = new StringBuilder();
                while (i < length) {
                    c = input.charAt(i);
                    if (c == '\'' || c == '\"') {
                        break;
                    } else {
                        sb.append(c);
                        i++;
                    }
                }
                tokens.add(sb.toString());
            }
        }
        return tokens;
    }


}

