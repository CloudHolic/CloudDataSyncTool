using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CloudSync.Commands;
using CloudSync.Models;
using CloudSync.Utils;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

namespace CloudSync.ViewModels
{
    public class DbSelectWindowViewModel : ViewModelBase
    {
        private readonly ICustomDialogManager _dialogManager;

        public ObservableCollection<Connection> Databases
        {
            get { return Get(() => Databases); }
            set { Set(() => Databases, value); }
        }

        public Connection CurItem
        {
            get { return Get(() => CurItem); }
            set { Set(() => CurItem, value); }
        }
        
        public DbSelectWindowViewModel()
        {
            Databases = new ObservableCollection<Connection>();

            foreach (var item in ConfigManager.Instance.Config.Connections)
                Databases.Add(DbSetting.ConvertToConnection(item.Key, item.Value));

            CurItem = null;

            _dialogManager = new CustomDialogManager(new MetroDialogSettings
            {
                AnimateHide = false,
                AnimateShow = false
            });
        }

        public ICommand NewCommand
        {
            get
            {
                return Get(() => NewCommand, new RelayCommand(() =>
                {
                    Databases.Add(new Connection());
                    CurItem = Databases.Last();
                }));
            }
        }

        public ICommand DeleteCommand
        {
            get
            {
                return Get(() => DeleteCommand, new RelayCommand(() =>
                {
                    var idx = Databases.IndexOf(CurItem);
                    Databases.RemoveAt(idx);
                    CurItem = idx > 0 ? Databases[idx - 1] : null;
                },() => Databases.Count > 0 && CurItem != null));
            }
        }

        public ICommand TestCommand
        {
            get
            {
                return Get(() => TestCommand, new RelayCommand<MetroWindow>(window =>
                {
                    var check = DbUtils.CheckConnection(CurItem);
                    _dialogManager.ShowMessageBox("Test Result", "Test " + (check ? "passed." : "failed."), window);
                }, window => Databases.Count > 0 && CurItem != null));
            }
        }

        public ICommand SaveCommand
        {
            get
            {
                return Get(() => SaveCommand, new RelayCommand(() =>
                {
                    ConfigManager.Instance.Config.Connections.Clear();
                    foreach (var (key, conn) in Databases.Select(Connection.ConvertToDbSetting))
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
