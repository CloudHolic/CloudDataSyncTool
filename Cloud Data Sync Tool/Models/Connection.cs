using System.ComponentModel;
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

        #region string Name;
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        #endregion
        
        public Connection()
        {
            Host = "localhost";
            Port = 3306;
            Id = Name = "";
            Password = new SecureString();
        }

        public Connection(string id, string password, string host = "localhost", int port = 3306, string database = "")
        {
            Host = host;
            Port = port;
            Id = id;
            Password = ConvertToSecureString(password);
            Name = database;
        }

        public Connection(Connection prevConnection)
        {
            Host = prevConnection.Host;
            Port = prevConnection.Port;
            Id = prevConnection.Id;
            Password = prevConnection.Password;
            Name = prevConnection.Name;
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion

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
