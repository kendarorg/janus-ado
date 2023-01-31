using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PgWireAdo.wire.client
{
    public class ParameterStatus:PGClientMessage
    {
        public bool IsMatching(NetworkStream stream)
        {
            throw new NotImplementedException();
        }

        public void Read(NetworkStream stream)
        {
            throw new NotImplementedException();
        }

        public string Key { get; private set; }
        public string Value { get; private set; }
    }
}
