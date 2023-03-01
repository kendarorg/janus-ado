using PgWireAdo.utils;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using PgWireAdo.ado;

namespace PgWireAdo.wire.server
{
    public abstract class PgwServerMessage
    {
        abstract public void Write(PgwByteBuffer stream);

        
    }
}
