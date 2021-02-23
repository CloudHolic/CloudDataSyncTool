using System;

namespace DbTest
{
    public class Connection
    {
        public string Id { get; set; }

        public string Password { get; set; }

        public string Host { get; set; }

        public string Port { get; set; }

        public string DbName { get; set; }

        public Connection(string id, string password, string host, int port = 3306, string dbName = "")
        {
            Id = id;
            Password = password;
            Host = host;
            Port = port.ToString();
            DbName = dbName;
        }
    }
}
