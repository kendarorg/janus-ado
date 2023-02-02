package org.example.messages.querymessage;

public class Field {
    private final String name;
    private final int tableObjectId;
    private final int columnAttributeNumber;

    public String getName() {
        return name;
    }

    public int getTableObjectId() {
        return tableObjectId;
    }

    public int getColumnAttributeNumber() {
        return columnAttributeNumber;
    }

    public int getDataTypeObjectId() {
        return dataTypeObjectId;
    }

    public int getDataTypeSize() {
        return precision;
    }

    public int getTypeModifier() {
        return typeModifier;
    }

    public int getFormatCode() {
        return formatCode;
    }

    private final int dataTypeObjectId;
    private final int precision;
    private final int typeModifier;
    private final int formatCode;

    public int getPrecision() {
        return precision;
    }

    public String getColumnClassName() {
        return columnClassName;
    }

    public int getScale() {
        return scale;
    }

    public int getSqlType() {
        return sqlType;
    }

    private String columnClassName;
    private int scale;
    private int sqlType;

    public Field(
            String name,
            int tableObjectId,
            int columnAttributeNumber,
            int dataTypeObjectId,
            int precision,
            int typeModifier,
            int formatCode,
            String columnClassName,
            int scale,int sqlType
            ){

        this.name = name;
        this.tableObjectId = tableObjectId;
        this.columnAttributeNumber = columnAttributeNumber;
        this.dataTypeObjectId = dataTypeObjectId;
        this.precision = precision;
        this.typeModifier = typeModifier;
        this.formatCode = formatCode;
        this.columnClassName = columnClassName;
        this.scale = scale;
        this.sqlType = sqlType;
    }

    public int length() {
        // 4 (int tableObjectId) + 2 (short columnAttributeNumber) + 4 (int dataTypeObjectId) + 2 (short dataTypeSize) + 4 (int typeModifier) + 2 (short formatCode)
        // 4 + 2 + 4 + 2 + 4 + 2 = 18
        // Add name length, plus 1 for null terminator '\0'
        return 18 + name.length() + 1;
    }
}
