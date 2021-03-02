using System;
using System.Windows.Input;
using CloudSync.Commands;
using CloudSync.Models;
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

        public bool SyncedWithSrc
        {
            get { return Get(() => SyncedWithSrc); }
            set { Set(() => SyncedWithSrc, value); }
        }

        public OpenWindowViewModel()
        {
            SrcCon = new Connection();
            DstCon = new Connection();
        }

        public ICommand SaveCommand
        {
            get
            {
                return Get(() => SaveCommand, new RelayCommand<MetroWindow>(window =>
                {
                    if (string.IsNullOrEmpty(SrcCon.Host))
                        SrcCon.Host = "localhost";
                    if (SrcCon.Port == 0)
                        SrcCon.Port = 3306;

                    if (SyncedWithSrc)
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
    }
}
