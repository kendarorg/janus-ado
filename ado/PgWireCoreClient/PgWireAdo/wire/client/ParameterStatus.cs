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
        public override BackendMessageCode BeType => BackendMessageCode.ParameterStatus;
        

        public override void Read(DataMessage stream)
        {
            ConsoleOut.WriteLine("ParameterStatus");
            Key = stream.ReadAsciiString();
            Value = stream.ReadAsciiString();
        }

        public string Key { get; private set; }
        public string Value { get; private set; }
    }
}
