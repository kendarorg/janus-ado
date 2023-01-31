using PgWireAdo.wire.client;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PgWireAdo.wire.server
{
    public class SSLNegotation : PGServerMessage
    {
        public void Write(NetworkStream stream)
        {
            //DO STUFF
            var sslResponse = new SSLResponse();
            if (sslResponse.IsMatching(stream))
            {
                sslResponse.Read(stream);
            }
        }
    }
}
