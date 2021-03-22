using CloudSync.Models;

namespace CloudSync.ViewModels
{
    public class DbSelectWindowViewModel : ViewModelBase
    {
        public Connection Conn
        {
            get { return Get(() => Conn); }
            set { Set(() => Conn, value); }
        }

        public DbSelectWindowViewModel()
        {
            Conn = new Connection();
        }
    }
}
