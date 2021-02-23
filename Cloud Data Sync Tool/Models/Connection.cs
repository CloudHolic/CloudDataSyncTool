using System.ComponentModel;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;

namespace CloudSync.Models
{
    public class Connection : INotifyPropertyChanged
    {
        #region string Host;
        private string _host;
        public string Host
        {
            get => _host;
            set
            {
                _host = value;
                OnPropertyChanged(nameof(Host));
            }
        }
        #endregion

        #region int Port;
        private int _port;
        public int Port
        {
            get => _port;
            set
            {
                _port = value;
                OnPropertyChanged(nameof(Port));
            }
        }
        #endregion

        #region string Id;
        private string _id;
        public string Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged(nameof(Id));
            }
        }
        #endregion

        #region SecureString Password;
        private SecureString _password;
        public SecureString Password
        {
            get => _password;
            set
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }
        #endregion

        #region string Database;
        private string _database;
        public string Database
        {
            get => _database;
            set
            {
                _database = value;
                OnPropertyChanged(nameof(Database));
            }
        }
        #endregion
        
        public Connection()
        {
            Host = "localhost";
            Port = 3306;
            Id = Database = "";
            Password = new SecureString();
        }

        public Connection(string id, string password, string host = "localhost", int port = 3306, string database = "")
        {
            Host = host;
            Port = port;
            Id = id;
            Password = ConvertToSecureString(password);
            Database = database;
        }

        public Connection(Connection prevConnection)
        {
            Host = prevConnection.Host;
            Port = prevConnection.Port;
            Id = prevConnection.Id;
            Password = prevConnection.Password;
            Database = prevConnection.Database;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private SecureString ConvertToSecureString(string password)
        {
            if (string.IsNullOrEmpty(password))
                return new SecureString();

            var secure = new SecureString();

            foreach (var c in password)
                secure.AppendChar(c);

            secure.MakeReadOnly();
            return secure;
        }

        public string GetPassword()
        {
            if (Password == null)
                return "";

            var pStr = Marshal.SecureStringToCoTaskMemUnicode(Password);
            var unsecuredPassword = Marshal.PtrToStringUni(pStr);
            Marshal.ZeroFreeCoTaskMemUnicode(pStr);

            return unsecuredPassword;
        }
    }
}
