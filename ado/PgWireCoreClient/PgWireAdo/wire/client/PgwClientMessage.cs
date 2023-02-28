using PgWireAdo.utils;
using System.Net.Sockets;

namespace PgWireAdo.wire.client;

public abstract class PgwClientMessage
{
    abstract public Boolean IsMatching(ReadSeekableStream stream);
    abstract public void Read(ReadSeekableStream stream);
    
    protected bool ReadData(ReadSeekableStream stream, Func<bool> func)
    {
        var ms = DateTimeOffset.Now.ToUnixTimeMilliseconds() + 10*1000;
        while (ms > DateTimeOffset.Now.ToUnixTimeMilliseconds())
        {
            try
            {
                var position = stream.Position;
                var result = func.Invoke();
                stream.Position = position;
                return result;
            }
            catch (Exception ex)
            {
                Thread.Sleep(10);
            }
        }

        throw new TimeoutException();

    }
}