using System.Net.Sockets;

namespace PgWireAdo.wire.client;

public interface PGClientMessage
{
    Boolean IsMatching(NetworkStream stream);
    void Read(NetworkStream stream);
}