using PgWireAdo.ado;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PgWireAdo.Test.Utils
{
    public class TestBase
    {
        [TearDown]
        public void Cleanup()
        {
            if (_connection == null) return;
            _connection.Close();
        }

        private Process _process;
        private bool _startServer = false;
        private DbConnection _connection;

        public int WriteBufferSize
        {
            get { return 256; }
        }

        public String ConnectionString
        {
            get { return "Host=localhost; Database=test;"; }
        }

        protected virtual PgwDataSource CreateDataSource()
        {
            return CreateDataSource(ConnectionString);
        }

        protected virtual PgwDataSource CreateDataSource(string connectionString)
            => PgwDataSource.Create(connectionString);

        protected virtual PgwDataSource CreateDataSource(Action<PgwConnectionStringBuilder> connectionStringBuilderAction)
        {
            var connectionStringBuilder = new PgwConnectionStringBuilder(ConnectionString);
            connectionStringBuilderAction(connectionStringBuilder);
            return PgwDataSource.Create(connectionStringBuilder.ConnectionString);
        }



        public bool IsMultiplexing {get{ return false;}}

        protected void TbRunBeforeAnyTests()
        {
            if (!_startServer) return;
            var javaHome = Environment.GetEnvironmentVariable("JAVA_HOME");
            var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            while (!Directory.Exists(Path.Combine(currentFolder, "pgwire-jdbc-server")))
            {
                currentFolder = Directory.GetParent(currentFolder).FullName;
            }

            var serverPath = Path.Combine(currentFolder, "pgwire-jdbc-server", "pwgire-server", "target");
            var jarFile = Directory.GetFiles(serverPath, "*.jar")[0];
            _process = new Process();
            var startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = Path.Combine(javaHome, "bin", "java");
            startInfo.Arguments = "-jar " + jarFile;
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
            _process.StartInfo = startInfo;
            _process.Start();
            Task.Run(() =>
            {
                while (!_process.StandardOutput.EndOfStream)
                {
                    string line = _process.StandardOutput.ReadLine();
                    System.Diagnostics.Trace.WriteLine(line);
                    TestContext.Progress.WriteLine(line);
                    // do something with line
                }
            });
            Thread.Sleep(1000);
        }

        protected void TbRunAfterAnyTests()
        {
            if (_process == null) return;
            _process.Kill(true);
        }

        

        protected virtual DbConnection OpenConnection()
        {
            var _connection = CreateConnection();
            try
            {
                OpenConnection(_connection, async: false).GetAwaiter().GetResult();
                return _connection;
            }
            catch
            {
                _connection.Dispose();
                throw;
            }
        }

        protected virtual DbConnection CreateConnection()
        {
            DbProviderFactory factory = PgwProviderFactory.Instance;
            DbConnection conn = factory.CreateConnection();
            conn.ConnectionString = "Host=localhost; Database=test;";
            return conn;
        }

        protected virtual async ValueTask<DbConnection> OpenConnectionAsync()
        {
            var connection = CreateConnection();
            try
            {
                await OpenConnection(connection, async: true);
                return connection;
            }
            catch
            {
                await connection.DisposeAsync();
                throw;
            }
        }

        protected static PgwCommand CreateSleepCommand(DbConnection conn, int seconds = 1000)
        => new($"SELECT pg_sleep({seconds})", conn);

        static Task OpenConnection(DbConnection conn, bool async)
        {
            return OpenConnectionInternal(hasLock: false);

            async Task OpenConnectionInternal(bool hasLock)
            {
                if (async)
                    await conn.OpenAsync();
                else
                    conn.Open();
            }
        }
    }
}
