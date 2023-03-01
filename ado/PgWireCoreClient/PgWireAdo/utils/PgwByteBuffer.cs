using PgWireAdo.wire.server;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;
using PgWireAdo.wire.client;
using ConcurrentLinkedList;
using PgWireAdo.ado;
using PgWireAdo.wire;
using System.IO;

namespace PgWireAdo.utils;

public class PgwByteBuffer
{
    //private readonly Socket _client;
    private readonly PgwConnection _connection;
    private readonly BufferedStream _stream;

    //private readonly BinaryWriter _sw;

    private readonly TcpClient _client;
    //private readonly StreamReader _sr;
    //private readonly StreamWriter _sw;

    public T? WaitFor<T>() where T : PgwClientMessage, new()
    {
        PgwClientMessage message = new T();
        DataMessage dm = null;
        var now = DateTimeOffset.Now.ToUnixTimeMilliseconds() + 1000;
        Node<DataMessage>? node = null;

        while (dm == null && _connection.Running)
        {

            var first = _connection.InputQueue.First;
            while (first != null && first.Value != null)
            {
                if (first.Value.Type == (byte)message.BeType)
                {
                    dm = first.Value;
                    node = first;
                }

                first = first.Next;
            }

            var after = now = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            if (after > now)
            {
                throw new Exception("Unable to find " + message.BeType + " message");
            }
            if (dm == null) Thread.Sleep(10);
        }
        if (dm != null)
        {
            Console.WriteLine("[SERVER] Recv:* " + dm.Type);
            DataMessage outMsg;
            _connection.InputQueue.Remove(dm, out outMsg);
            if (outMsg == null)
            {
                return null;
            }
            message.Read(outMsg);
            return (T)message;
        }

        return null;
    }

    public PgwByteBuffer(TcpClient client, PgwConnection connection)
    {
        //_client = client;
        //tcp.GetStream()
        //client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        _connection = connection;
        _stream = new BufferedStream(client.GetStream(),2000000);
        
        //_sr = new StreamReader(_stream);
       // _sw = new BinaryWriter(_stream);
    }

    public void Write(PgwServerMessage message)
    {
        message.Write(this);
        _stream.Flush();
        //_sw.Flush();
        
        

        
    }

    public void WriteByte(byte value)
    {
        _stream.Write(new []{value},0,1);
    }

    public void WriteInt16(short value)
    {
        var val = BitConverter.GetBytes(BinaryPrimitives.ReverseEndianness(value));
        _stream.Write(val);
    }

    public void WriteInt32(int value)
    {
        var val = BitConverter.GetBytes(BinaryPrimitives.ReverseEndianness(value));
        _stream.Write(val);
    }

    public void WriteASCIIString(String value)
    {
        var val = Encoding.ASCII.GetBytes(value);
        _stream.Write(val);
    }

    public void Flush()
    {
        _stream.Flush();
    }

    public void Write(byte[] val)
    {
        _stream.Write(val);

    }

    public byte[] Read(int size)
    {
        var tmpData = new byte[size];
        var localBuffer = new byte[1024];
        var redBytes = 0;
        /*while (redBytes < size)
        {
            var partial = _stream.Read(tmpData, 0, size);
            if (partial == 0 && redBytes != size) throw new IOException("Missing data");
            for (var i = 0; i < partial; i++, redBytes++)
            {
                tmpData[redBytes] = (byte)localBuffer[i];
            }
        }*/
        var partial = _stream.Read(tmpData, 0, size);
        return tmpData;
    }

    public byte ReadByte()
    {
        var res = new byte[1];
            _stream.Read(res);
            return res[0];
    }



    public short ReadInt16()
    {
        var res = new byte[2];
        _stream.Read(res);
        return BinaryPrimitives.ReverseEndianness(BitConverter.ToInt16(res, 0));
    }
    public int ReadInt32()
    {
        var res = new byte[4];
        _stream.Read(res);
        return BinaryPrimitives.ReverseEndianness(BitConverter.ToInt32(res, 0));
    }
    /*
    public void Read(PgwClientMessage pgwClientMessage)
    {
        Console.WriteLine("[SERVER] Recv: " + pgwClientMessage.GetType().Name);
        pgwClientMessage.Read(this);
    }*/

    public List<String> ReadStrings(int bufferLength)
    {
        var result = new List<String>();
        var data = Read(bufferLength);
        var start = 0;
        var count = 0;
        var ms = new MemoryStream();
        for (var i = 0; i < data.Length; i++)
        {
            count++;
            if (data[i] == 0x00)
            {
                ms.Write(data, start, count);
                result.Add(ASCIIEncoding.Default.GetString(ms.ToArray()));
                ms = new MemoryStream();
                i++;
                start = i;
                count = 0;
            }
            
        }

        return result;
    }
}