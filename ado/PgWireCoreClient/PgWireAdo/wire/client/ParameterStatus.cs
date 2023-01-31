using PgWireAdo.utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PgWireAdo.wire.client
{
    public class ParameterStatus:PgwClientMessage
    {
        public override bool IsMatching(ReadSeekableStream stream)
        {
            throw new NotImplementedException();
        }

        public override void Read(ReadSeekableStream stream)
        {
            throw new NotImplementedException();
        }

        public string Key { get; private set; }
        public string Value { get; private set; }
    }
}
