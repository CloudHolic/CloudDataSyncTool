using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CloudSync.Commands;
using CloudSync.Models;
using CloudSync.Utils;
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

        private readonly List<Connection> _connections;

        public DbSelectWindowViewModel()
        {
            Databases = new ObservableCollection<string>();
            _connections = new List<Connection>();
            foreach (var item in ConfigManager.Instance.Config.Connections)
            {
                Databases.Add(item.Key);
                _connections.Add(DbSetting.ConvertToConnection(item.Key, item.Value));
            }

            Conn = new Connection("", "", "", 0, "");
        }

        public ICommand NewCommand
        {
            get
            {
                return Get(() => NewCommand, new RelayCommand(() =>
                {
                    Databases.Add("New");
                    _connections.Add(new Connection());
                    CurIdx = Databases.Count - 1;
                    Conn = _connections[_connections.Count - 1];
                }));
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                return Get(() => DeleteCommand, new RelayCommand(() =>
                {
                    Databases.RemoveAt(CurIdx);
                    _connections.RemoveAt(CurIdx);
                    CurIdx -= 1;
                    Conn = CurIdx > -1 ? _connections[CurIdx] : new Connection("", "", "", 0, "");
                }));
            }
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
                    ConfigManager.Instance.Config.Connections.Clear();
                    foreach (var (key, conn) in _connections.Select(Connection.ConvertToDbSetting))
                        ConfigManager.Instance.Config.Connections.Add(key, conn);

                    ConfigManager.Instance.Save();
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
