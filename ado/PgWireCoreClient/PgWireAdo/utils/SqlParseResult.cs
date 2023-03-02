namespace PgWireAdo.utils;

public class SqlParseResult
{
    public SqlParseResult(string value, SqlStringType type)
    {
        Value = value;
        Type = type;
    }

    public string Value { get; set; }
    public SqlStringType Type { get; set; }
}