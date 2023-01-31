using PgWireAdo.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PgWireAdo.wire.client
{
    public class ReadyForQuery:PgwClientMessage
    {
        public bool IsMatching(ReadSeekableStream stream)
        {
            throw new NotImplementedException();
        }

        public void Read(ReadSeekableStream stream)
        {
            throw new NotImplementedException();
        }
    }
}
