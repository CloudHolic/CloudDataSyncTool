using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CloudSync.Commands;
using CloudSync.Contents;
using CloudSync.Models;
using CloudSync.Utils;
using MahApps.Metro.Controls;

namespace CloudSync.ViewModels
{
    public class OpenWindowViewModel : ViewModelBase
    {
        public Connection SrcCon
        {
            get { return Get(() => SrcCon); }
            set { Set(() => SrcCon, value); }
        }

        public Connection DstCon
        {
            get { return Get(() => DstCon); }
            set { Set(() => DstCon, value); }
        }

        public bool IsSrcCustom
        {
            get { return Get(() => IsSrcCustom); }
            set { Set(() => IsSrcCustom, value); }
        }

        public bool IsDstCustom
        {
            get { return Get(() => IsDstCustom); }
            set { Set(() => IsDstCustom, value); }
        }

        public bool IsSyncedWithSrc
        {
            get { return Get(() => IsSyncedWithSrc); }
            set { Set(() => IsSyncedWithSrc, value); }
        }

        public Connection SrcPreset
        {
            get { return Get(() => SrcPreset); }
            set { Set(() => SrcPreset, value); }
        }

        public Connection DstPreset
        {
            get { return Get(() => DstPreset); }
            set { Set(() => DstPreset, value); }
        }

        public ObservableCollection<Connection> Databases
        {
            get { return Get(() => Databases); }
            set { Set(() => Databases, value); }
        }

        public OpenWindowViewModel()
        {
            LoadSettings();
        }

        public ICommand SrcSelChangedCommand
        {
            get
            {
                return Get(() => SrcSelChangedCommand, new RelayCommand(() =>
                {
                    if (Databases.Count == 0)
                        return;

                    SrcCon = new Connection(SrcPreset);
                    IsSrcCustom = SrcPreset.Name == "Custom";
                    if (IsSrcCustom)
                        SrcCon.Name = "";
                    if (!(IsSrcCustom && IsDstCustom))
                        IsSyncedWithSrc = false;
                }));
            }
        }

        public ICommand DstSelChangedCommand
        {
            get
            {
                return Get(() => DstSelChangedCommand, new RelayCommand(() =>
                {
                    if (Databases.Count == 0)
                        return;

                    DstCon = new Connection(DstPreset);
                    IsDstCustom = DstPreset.Name == "Custom";
                    if (IsDstCustom)
                        DstCon.Name = "";
                    if (!(IsSrcCustom && IsDstCustom))
                        IsSyncedWithSrc = false;
                }));
            }
        }

        public ICommand SettingCommand
        {
            get
            {
                return Get(() => SettingCommand, new RelayCommand(() =>
                {
                    var openWindow = new DbSelectWindow();
                    openWindow.ShowDialog();
                    LoadSettings();
                }));
            }
        }

        public ICommand OkCommand
        {
            get
            {
                return Get(() => OkCommand, new RelayCommand<MetroWindow>(window =>
                {
                    if (string.IsNullOrEmpty(SrcCon.Host))
                        SrcCon.Host = "localhost";
                    if (SrcCon.Port == 0)
                        SrcCon.Port = 3306;

                    if (IsSyncedWithSrc)
                    {
                        var temp = DstCon.Name;
                        DstCon = new Connection(SrcCon) {Name = temp};
                    }

                    if (string.IsNullOrEmpty(DstCon.Host))
                        DstCon.Host = "localhost";
                    if (DstCon.Port == 0)
                        DstCon.Port = 3306;

                    window.Tag = new Tuple<Connection, Connection>(new Connection(SrcCon), new Connection(DstCon));
                    window.Close();
                }, window => !string.IsNullOrEmpty(SrcCon.Host) || !string.IsNullOrEmpty(DstCon.Host)));
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

        private void LoadSettings()
        {
            Databases = new ObservableCollection<Connection>();
            
            ConfigManager.Instance.Load();
            foreach (var item in ConfigManager.Instance.Config.Connections)
                Databases.Add(DbSetting.ConvertToConnection(item.Key, item.Value));

            Databases.Add(new Connection("", "", "", 3306, "Custom"));

            SrcPreset = DstPreset = Databases.LastOrDefault();
            IsSrcCustom = IsDstCustom = true;
            IsSyncedWithSrc = false;

            SrcCon = new Connection(SrcPreset) { Name = "" };
            DstCon = new Connection(DstPreset) { Name = "" };
        }
    }
}
