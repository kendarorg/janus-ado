using PgWireAdo.utils;
using System.Net.Sockets;

namespace PgWireAdo.wire.client;

public abstract class PgwClientMessage
{
    protected bool Equals(PgwClientMessage other)
    {
        return BeType == other.BeType && _uuid==other._uuid;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((PgwClientMessage)obj);
    }

    public override int GetHashCode()
    {
        return (int)BeType;
    }

    private Guid _uuid = new Guid();
    abstract public void Read(DataMessage stream);
    abstract public BackendMessageCode BeType { get; }
}