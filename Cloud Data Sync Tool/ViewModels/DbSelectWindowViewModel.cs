using System.Collections.ObjectModel;
using System.Windows.Input;
using CloudSync.Commands;
using CloudSync.Models;
using MahApps.Metro.Controls;

namespace CloudSync.ViewModels
{
    public class DbSelectWindowViewModel : ViewModelBase
    {
        public Connection Conn
        {
            get { return Get(() => Conn); }
            set { Set(() => Conn, value); }
        }

        public ObservableCollection<string> Databases
        {
            get { return Get(() => Databases); }
            set { Set(() => Databases, value); }
        }

        public int CurIdx
        {
            get { return Get(() => CurIdx); }
            set { Set(() => CurIdx, value); }
        }

        public DbSelectWindowViewModel()
        {
            Conn = new Connection();
            Databases = new ObservableCollection<string>();
            Conn = new Connection();
            CurIdx = 0;
        }

        public ICommand TestCommand
        {
            get
            {
                return Get(() => TestCommand, new RelayCommand(() =>
                {
                    
                }));
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                return Get(() => SaveCommand, new RelayCommand(() =>
                {
                    
                }));
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                return Get(() => DeleteCommand, new RelayCommand(() =>
                {

                }));
            }
        }

        public ICommand CloseCommand
        {
            get
            {
                return Get(() => CloseCommand, new RelayCommand<MetroWindow>(window =>
                {
                    window.Close();
                }));
            }
        }
    }
}
