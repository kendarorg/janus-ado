using System.Collections.Concurrent;
using System.Data;
using System.Data.Common;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using ConcurrentLinkedList;
using PgWireAdo.utils;
using PgWireAdo.wire;
using PgWireAdo.wire.client;
using PgWireAdo.wire.server;

namespace PgWireAdo.ado;

public class PgwConnection : DbConnection
{
    private PgwConnectionString _options;
    private string _connectionString;
    private ConnectionState _state = ConnectionState.Closed;
    private Socket _client;
    private PgwByteBuffer _byteBuffer;

    public PgwByteBuffer Stream { get { return _byteBuffer; } }

    public PgwConnection(string connectionString)
    {
        ConnectionString = connectionString;
    }

    public PgwConnection()
    {
        
    }

    private ConcurrentDictionary<Guid,DataMessage> inputQueue = new();
    private Thread _queueThread;

    public ConcurrentDictionary<Guid,DataMessage> InputQueue { get { return inputQueue; } }



    public override void Open()
    {
        
        _tcpClient = new TcpClient(_options.DataSource, _options.Port);
        /*_tcpClient.ReceiveTimeout = 2000;
        _tcpClient.SendTimeout = 2000;
        _tcpClient.ReceiveBufferSize = 2000000;
        _tcpClient.SendBufferSize = 2000000;
        //_tcpClient.NoDelay=true;
        //_tcpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        */
        _byteBuffer = new PgwByteBuffer(_tcpClient, this);
        

        _queueThread = new Thread(() =>
        {
            
            ReadDataFromStream(_byteBuffer);
        });
        _queueThread.Start();
        Task.Delay(10).Wait();
        _byteBuffer.Write(new SSLNegotation());
        var response = _byteBuffer.WaitFor<SSLResponse>();
        
         var parameters = new Dictionary<String, String>();
         parameters.Add("database", Database);
         var startup = new StartupMessage(parameters);
         _byteBuffer.Write(startup);
         _state = ConnectionState.Open;
    }


    public override Task OpenAsync(CancellationToken cancellationToken)
    {
        Open();
        return Task.CompletedTask;
    }

    protected override void Dispose(bool disposing)
    {
        if (_tcpClient != null && _tcpClient.Connected)
        {
            _byteBuffer.WriteSync(new TerminateMessage());
            Thread.Sleep(50);
        }
        _running = false;
        if (_state != ConnectionState.Closed)
        {
            _state = ConnectionState.Closed;
            if(_client!=null) { 
                _client.Dispose();
            }

            if (_tcpClient != null)
            {
                _tcpClient.Dispose();
            }
            
        }

        base.Dispose(disposing);
    }

    public override string ConnectionString {
        get { return _connectionString; }
        set
        {
            if (value == null)
            {
                value = string.Empty;
            }

            _options = new PgwConnectionString(value);
            _connectionString = value;
        }
    }
    public override string Database => _options.Database;
    public override ConnectionState State => _state;
    public override string DataSource => _options.DataSource;
    public override string ServerVersion => _options.ServerVersion;
    


    public override void Close()
    {
        _state = ConnectionState.Closed;
        
        _byteBuffer.WriteSync(new TerminateMessage());
        _state = ConnectionState.Closed;
        if (_client != null)
        {
            _client.Dispose();
            _client = null;
        }

        if (_tcpClient != null)
        {
            _tcpClient.Dispose();
            _tcpClient = null;
        }

    }


    protected override DbCommand CreateDbCommand()
    {
        return new PgwCommand(this);
    }

    public override ValueTask DisposeAsync()
    {
        return new ValueTask(Task.Run(Dispose));
    }

    #region TOIMPLEMENT

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
    {
        var result= new PgwTransaction(this, isolationLevel);
        _byteBuffer.Write(new QueryMessage("JANUS:BEGIN_TRANSACTION"));
        var cc = _byteBuffer.WaitFor<CommandComplete>();
        var rq = _byteBuffer.WaitFor<ReadyForQuery>();

        
        
        return result;
    }

    public override void ChangeDatabase(string databaseName)
    {
        throw new NotImplementedException();
    }

    #endregion

    private bool _running = true;
    private TcpClient _tcpClient;
    private static readonly char[] _values;

    static PgwConnection()
    {
        _values = Enum.GetValues(typeof(BackendMessageCode))
            .OfType<BackendMessageCode>()
            .Select(s => (char)s).ToArray();
    }

    private static void IsValidMessage(char s)
    {
        for (var index = 0; index < _values.Length; index++)
        {
            if (_values[index] == s)
            {
                return;
            }
        }

        throw new InvalidOperationException("Message type not allowed " + s);
    }

    public bool Running { get { return _running; } }
    private void ReadDataFromStream(PgwByteBuffer buffer)
    {
        try
        {
            long timestamp = 1L;
            var sslNegotiationDone = false;
            var header = new byte[5];
            while (_running && _tcpClient.Connected)
            {
                var messageType = (char)buffer.ReadByte();

                ConsoleOut.WriteLine("READ " + (char)messageType);

                IsValidMessage(messageType);
                if (messageType == 'N' && sslNegotiationDone == false)
                {
                    sslNegotiationDone = true;
                    var sslN = new DataMessage((char)messageType, 0, new byte[0], timestamp++);
                    inputQueue.AddOrUpdate(sslN.Id,sslN,(g,d)=> sslN);
                    continue;
                }

                var messageLength = buffer.ReadInt32();
                var data = new byte[0];
                if (messageLength > 4)
                {
                    data = buffer.Read(messageLength - 4);
                }

                var dm = new DataMessage((char)messageType, messageLength, data, timestamp++);
                inputQueue.AddOrUpdate(dm.Id, dm, (g, d) => dm);
            }
        }
        catch (Exception ex)
        {
            _running = false;
            if (_tcpClient != null)
            {
            _tcpClient.Close();
            _tcpClient=null;
        }
    }
}
}

