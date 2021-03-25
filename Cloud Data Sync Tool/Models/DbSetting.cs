namespace CloudSync.Models
{
    public class DbSetting
    {
        public string Host { get; set; }

        public int Port { get; set; }

        public string Id { get; set; }

        public string Password { get; set; }

        public DbSetting()
        {
            Host = "localhost";
            Port = 0;
            Id = Password = string.Empty;
        }

        public DbSetting(string host, int port, string id, string password)
        {
            Host = host;
            Port = port;
            Id = id;
            Password = password;
        }

        public DbSetting(DbSetting prevDbSetting)
        {
            Host = prevDbSetting.Host;
            Port = prevDbSetting.Port;
            Id = prevDbSetting.Id;
            Password = prevDbSetting.Password;
        }

        public static Connection ConvertToConnection(string dbName, DbSetting setting)
        {
            return new Connection(setting.Id, setting.Password, setting.Host, setting.Port, dbName);
        }
    }
}
