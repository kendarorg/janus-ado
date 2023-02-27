package org.example;

import java.util.*;
import java.util.concurrent.ConcurrentHashMap;

public class StringParser {

    /*public static void main(String[] args) {
        String input = "Hello, 'world!'";
        String[] tokens = parseString(input);
        for (String token : tokens) {
            System.out.println(token);
        }
    }*/
    static Map<String,SqlStringType> select;
    static {
        select = new ConcurrentHashMap<>();
        select.put("select",SqlStringType.SELECT);
        select.put("show",SqlStringType.SELECT);
        select.put("explain",SqlStringType.SELECT);
        select.put("describe",SqlStringType.SELECT);
        select.put("fetch",SqlStringType.SELECT);

        select.put("update",SqlStringType.UPDATE);
        select.put("insert",SqlStringType.UPDATE);
        select.put("create",SqlStringType.UPDATE);
        select.put("delete",SqlStringType.UPDATE);
        select.put("merge",SqlStringType.UPDATE);
        select.put("alter",SqlStringType.UPDATE);
        select.put("drop",SqlStringType.UPDATE);
        select.put("create",SqlStringType.UPDATE);
        select.put("grant",SqlStringType.UPDATE);
        select.put("set",SqlStringType.UPDATE);
        select.put("truncate",SqlStringType.UPDATE);
        select.put(":janus:",SqlStringType.UPDATE);
        select.put("declare",SqlStringType.UPDATE);

        select.put("call",SqlStringType.CALL);
        select.put("execute",SqlStringType.CALL);
        select.put("run",SqlStringType.CALL);
    }

    public static boolean isUnknown(List<SqlParseResult> data){
        return data.stream().anyMatch(a->a.getType()==SqlStringType.UNKNOWN||a.getType()==SqlStringType.NONE);
    }

    public static List<SqlParseResult> getTypes(String input){
        var result = new ArrayList<SqlParseResult>();
        var sqls=parseSql(input);
        for(var sql:sqls){
            var splitted = sql.trim().split("\\s+");
            if(splitted.length==0){
                continue;
            }
            var first = splitted[0].trim().toLowerCase(Locale.ROOT);
            if(first.length()==0){
                continue;
            }
            if(select.containsKey(first)){
                var founded = select.get(first);
                result.add(new SqlParseResult(sql,founded));
            }else{
                result.add(new SqlParseResult(sql,SqlStringType.UNKNOWN));
            }
        }
        return result;
    }

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


    public static boolean isMixed(List<SqlParseResult> parsed) {
        SqlStringType founded = SqlStringType.NONE;
        for (var single :parsed) {
            if(founded==SqlStringType.NONE){
                founded=single.getType();
            }
            if(single.getType()!=founded)return true;

        }
        return false;
    }
}

