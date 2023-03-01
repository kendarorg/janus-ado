package org.kendar.pgwire.utils;

public class SqlParseResult {
    private String value;
    private SqlStringType type;

    public SqlParseResult(String value, SqlStringType type)
    {
        this.value = value;
        this.type = type;
    }

    public String getValue() {
        return value;
    }

    public SqlStringType getType() {
        return type;
    }
}
