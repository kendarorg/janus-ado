using PgWireAdo.utils;

namespace PgWireAdo.wire.server;

public class DescribeMessage : PgwServerMessage
{
    private readonly char _type;
    private readonly string _id;

    public DescribeMessage(char type, string id)
    {
        _type = type;
        _id = id;
    }

    public override void Write(ReadSeekableStream stream)
    {
        ConsoleOut.WriteLine("DescribeMessage " );
        int length =  4 + 1 + _id.Length + 1;
        WriteByte((byte)'D');
        WriteInt32(length); 
        WriteByte((byte)_type);
        WriteASCIIString(_id);
        WriteByte(0);
        Flush(stream);
    }
}