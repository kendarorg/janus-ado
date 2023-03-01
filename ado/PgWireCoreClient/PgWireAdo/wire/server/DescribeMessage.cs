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

    public override void Write(PgwByteBuffer stream)
    {
        ConsoleOut.WriteLine("DescribeMessage " );
        int length =  4 + 1 + _id.Length + 1;
         stream.WriteByte((byte)'D');
        stream.WriteInt32(length); 
        stream.WriteByte((byte)_type);
        stream.WriteASCIIString(_id);
        stream.WriteByte(0);
    }
}