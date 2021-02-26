using System;
using CloudSync.Models;

namespace CloudSync.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Connection _srcConnection, _dstConnection;

        public MainWindowViewModel(Tuple<Connection, Connection> cons)
        {
            (_srcConnection, _dstConnection) = cons;
        }
    }
}
