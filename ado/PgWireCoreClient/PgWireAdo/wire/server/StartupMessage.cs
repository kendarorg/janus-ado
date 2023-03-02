using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using PgWireAdo.utils;
using PgWireAdo.wire.client;

namespace PgWireAdo.wire.server
{
    public class StartupMessage : PgwServerMessage
    {
        private readonly Dictionary<string, string> _parameters;
        readonly Dictionary<string, string> _serverParameters = new ();

        public StartupMessage(Dictionary<string, string> parameters)
        {
            _parameters = parameters;
        }

        public IDictionary<string, string> ServerParameters => _serverParameters;

        public override void Write(PgwByteBuffer stream)
        {
            ConsoleOut.WriteLine("[SERVER] Read: StartupMessage");
            var data = new List<byte[]>();
            int length = 4 + 4;
            foreach (var parameter in _parameters)
            {
                var k = Encoding.ASCII.GetBytes(parameter.Key);
                var v = Encoding.ASCII.GetBytes(parameter.Value);
                data.Add(k);
                data.Add(new byte[]{0});
                data.Add(v);
                data.Add(new byte[] { 0 });
                length += k.Length+1;
                length += v.Length+1;
            }
            stream.WriteInt32(length);
            stream.WriteByte(0x03);
            stream.WriteByte(0x00);
            stream.WriteByte(0x00);
            stream.WriteByte(0x00);
            foreach (byte[] row in data)
            {
                stream.Write(row);
            }
            stream.Flush();

            //SEND THE MESSAGE PLUS PARAMETERS
            var authenticationOk =stream.WaitFor<AuthenticationOk>();
            var backendKeyData =stream.WaitFor<BackendKeyData>();
            var parameterStatus = stream.WaitFor<ParameterStatus>();
            while (parameterStatus != null)
            {
                _serverParameters.Add(parameterStatus.Key, parameterStatus.Value);
                parameterStatus = stream.WaitFor<ParameterStatus>(timeout:10L);
            }

            var readyForQuery = stream.WaitFor<ReadyForQuery>();

            if (authenticationOk == null || readyForQuery == null || backendKeyData == null)
            {
                throw new InvalidOperationException("[ERROR] StartupMessage");
            }
        }
    }
}
