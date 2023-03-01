using PgWireAdo.wire.server;
using System.Buffers.Binary;
using System.Net.Sockets;
using System.Text;
using PgWireAdo.wire.client;
using ConcurrentLinkedList;
using PgWireAdo.ado;
using PgWireAdo.wire;
using System.IO;
using System.Xml.Linq;

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

    public T? WaitFor<T>(Action<T> preRead=null) where T : PgwClientMessage, new()
    {
        PgwClientMessage message = new T();
        DataMessage dm = null;
        var now = DateTimeOffset.Now.ToUnixTimeMilliseconds() + 2000;
        

        while (dm == null && _connection.Running)
        {

            foreach (var dataMessage in _connection.InputQueue.ToArray())
            {
                if (dataMessage.Type == (byte)BackendMessageCode.ErrorResponse)
                {
                    dm = dataMessage;
                    break;
                }
                if (dataMessage.Type == (byte)message.BeType)
                {
                    if(dm!=null && dm.Timestamp>dataMessage.Timestamp)continue;
                    dm = dataMessage;
                }
            }
            

            var after =  DateTimeOffset.Now.ToUnixTimeMilliseconds();
            if (after > now)
            {
                throw new Exception("Unable to find " + message.BeType + " message");
            }
            if (dm == null) Thread.Sleep(10);
        }
        if (dm != null)
        {
            
            
            DataMessage outMsg;
            while (!_connection.InputQueue.TryTake(out outMsg))
            {
                Thread.Sleep(10);
            }
            if (outMsg == null)
            {
                return null;
            }
            if (outMsg.Type == (byte)BackendMessageCode.ErrorResponse)
            {
                message = new ErrorResponse();
            }
            else
            {
                if (preRead != null)
                {
                    preRead.Invoke((T)message);
                }
            }
            Console.WriteLine("[SERVER] Recv:* " + message.GetType().Name);
            message.Read(outMsg);
            return (T)message;
        }

        return null;
    }

    public PgwClientMessage WaitFor<T,K>(Action<T> preAction=null) where T : PgwClientMessage, new() where K : PgwClientMessage, new()
    {
        PgwClientMessage message = null;
        PgwClientMessage message1 = new T();
        PgwClientMessage message2 = new K();
        DataMessage dm = null;
        var now = DateTimeOffset.Now.ToUnixTimeMilliseconds() + 1000L;


        while (dm == null && _connection.Running)
        {

            foreach (var dataMessage in _connection.InputQueue.ToArray())
            {
                if (dataMessage.Type == (byte)BackendMessageCode.ErrorResponse)
                {
                    dm = dataMessage;
                    break;
                }
                if (dataMessage.Type == (byte)message1.BeType)
                {
                    if (dm != null && dm.Timestamp > dataMessage.Timestamp) continue;
                    dm = dataMessage;
                }else if (dataMessage.Type == (byte)message2.BeType)
                {
                    if (dm != null && dm.Timestamp > dataMessage.Timestamp) continue;
                    dm = dataMessage;
                }
            }

            var after =  DateTimeOffset.Now.ToUnixTimeMilliseconds();
            if (after > now)
            {
                throw new Exception("Unable to find " + message1.BeType + " message or "+ message2.BeType);
            }
            if (dm == null) Thread.Sleep(10);
        }
        if (dm != null)
        {
            DataMessage outMsg;
            while (!_connection.InputQueue.TryTake(out outMsg))
            {
                Thread.Sleep(10);
            }
            if (outMsg == null)
            {
                return null;
            }
            if (outMsg.Type == (byte)BackendMessageCode.ErrorResponse)
            {
                message = new ErrorResponse();
            }
            else if (outMsg.Type == (byte)message2.BeType)
            {
                message = message2;
            }
            else if (outMsg.Type == (byte)message1.BeType)
            {
                if(preAction!=null)preAction.Invoke((T)message1);
                message = message1;
            }
            Console.WriteLine("[SERVER] Recv:* " + message.GetType().Name);
            message.Read(outMsg);
            return message;
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


    private static void ReadFully(Stream inputStream, byte[] buffer)
    {
        if (inputStream == null)
        {
            throw new ArgumentNullException("inputStream");
        }

        if (buffer == null)
        {
            throw new ArgumentNullException("buffer");
        }

        int totalBytesRead = 0;
        int bytesLeft = buffer.Length;
        if (bytesLeft <= 0)
        {
            throw new ArgumentException("There is nothing to read for the specified buffer", "buffer");
        }

        while (totalBytesRead < buffer.Length)
        {
            var bytesRead = inputStream.Read(buffer, totalBytesRead, bytesLeft);
            if (bytesRead > 0)
            {
                totalBytesRead += bytesRead;
                bytesLeft -= bytesRead;
            }
            else
            {
                throw new InvalidOperationException("Input stream reaches the end before reading all the bytes");
            }
        }
    }

    public void Write(PgwServerMessage message)
    {
        message.Write(this);
        _stream.FlushAsync().Wait();
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
        ReadFully(_stream, tmpData);// _stream.Read(tmpData, 0, size);
        return tmpData;
    }

    public byte ReadByte()
    {
        var res = new byte[1];
        ReadFully(_stream, res);
            return res[0];
    }



    public short ReadInt16()
    {
        var res = new byte[2];
        ReadFully(_stream, res);
        return BinaryPrimitives.ReverseEndianness(BitConverter.ToInt16(res, 0));
    }
    public int ReadInt32()
    {
        var res = new byte[4];
        ReadFully(_stream, res);
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