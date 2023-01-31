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

        public void Write(ReadSeekableStream stream)
        {
            //SEND THE MESSAGE PLUS PARAMETERS
            var authenticationOk = new AuthenticationOk();
            if (authenticationOk.IsMatching(stream))
            {
                authenticationOk.Read(stream);
                var backendKeyData = new BackendKeyData();
                if (backendKeyData.IsMatching(stream))
                {
                    backendKeyData.Read(stream);
                    var parameterStatus = new ParameterStatus();
                    while (parameterStatus.IsMatching(stream))
                    {
                        parameterStatus.Read(stream);
                        _serverParameters.Add(parameterStatus.Key,parameterStatus.Value);
                    }

                    var readyForQuery = new ReadyForQuery();
                    if (readyForQuery.IsMatching(stream))
                    {
                        readyForQuery.Read(stream);
                        return;
                    }
                }
            }
            throw new Exception("[ERROR] StartupMessage");
        }
    }
}
