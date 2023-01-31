using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PgWireAdo.wire.client
{
    public class AuthenticationOk:PGClientMessage
    {
        public bool IsMatching(NetworkStream stream)
        {
            throw new NotImplementedException();
        }

        public void Read(NetworkStream stream)
        {
            throw new NotImplementedException();
        }
    }
}
