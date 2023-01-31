using PgWireAdo.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PgWireAdo.wire.server
{
    public class TerminateMessage : PgwServerMessage
    {
        public void Write(ReadSeekableStream stream)
        {
            //DO STUFF
            throw new NotImplementedException();
        }
    }
}
