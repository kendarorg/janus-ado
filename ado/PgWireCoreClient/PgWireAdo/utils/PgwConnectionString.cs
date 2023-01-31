using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PgWireAdo.utils
{
    public class PgwConnectionString
    {
        public PgwConnectionString()
        {
            DataSource = "localhost";
            Port = 5432;
            ServerVersion = "15";
            ConnectionTimeout = 1000;
        }
        public PgwConnectionString(string value):this()
        {
            Load(value);
        }

        private void Load(string connectionString)
        {
            var splitted = connectionString.Split(";");
            foreach (var kv in splitted)
            {
                var sub = kv.Split("=");
                var k = sub[0].ToLowerInvariant();
                var v = sub[1];
                switch (k)
                {
                    case ("database"):
                        Database = v;
                        break;
                    case ("port"):
                        Port = Int32.Parse(v);
                        break;
                    case ("timeout"):
                        ConnectionTimeout = Int32.Parse(v);
                        break;
                    case ("server"):
                        DataSource =v;
                        break;
                }
            }
        }

        public string Database { get; private set; }
        public int Port { get; set; }
        public string DataSource { get; set; }
        public string ServerVersion { get; set; }
        public int ConnectionTimeout { get; set; }
    }
}
