using PgWireAdo.utils;
using System.Net.Sockets;

namespace PgWireAdo.wire.client;

public abstract class PgwClientMessage
{
    abstract public void Read(DataMessage stream);
    abstract public BackendMessageCode BeType { get; }
}