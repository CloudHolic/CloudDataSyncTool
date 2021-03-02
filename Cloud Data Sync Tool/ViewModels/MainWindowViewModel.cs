using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CloudSync.Commands;
using CloudSync.Contents;
using CloudSync.Models;
using CloudSync.Utils;

namespace CloudSync.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Connection _srcConnection, _dstConnection;

        public string SrcName
        {
            get { return Get(() => SrcName); }
            set { Set(() => SrcName, value); }
        }

        public ObservableCollection<TableList> SrcTables
        {
            get { return Get(() => SrcTables); }
            set { Set(() => SrcTables, value); }
        }
        
        public string DstName
        {
            get { return Get(() => DstName); }
            set { Set(() => DstName, value); }
        }

        public ObservableCollection<TableList> DstTables
        {
            get { return Get(() => DstTables); }
            set { Set(() => DstTables, value); }
        }

        public MainWindowViewModel(Tuple<Connection, Connection> cons)
        {
            (_srcConnection, _dstConnection) = cons;
            var dbUtil = new DbUtils(_srcConnection, _dstConnection);
            SrcTables = new ObservableCollection<TableList>();
            DstTables = new ObservableCollection<TableList>();

            SrcName = _srcConnection.Name;
            DstName = _dstConnection.Name;

            var srcSchemas = dbUtil.FindSchemas();
            var dstSchemas = dbUtil.FindSchemas(false);

            foreach (var schema in srcSchemas)
            {
                SrcTables.Add(new TableList
                {
                    SchemaName = schema,
                    TableNames = new ObservableCollection<string>(dbUtil.FindTables(schema))
                });
            }

            foreach (var schema in dstSchemas)
            {
                DstTables.Add(new TableList
                {
                    SchemaName = schema,
                    TableNames = new ObservableCollection<string>(dbUtil.FindTables(schema, false))
                });
            }
        }

        public ICommand CopyCommand
        {
            get
            {
                return Get(() => CopyCommand, new RelayCommand(() =>
                { 

                }));
            }
        }

        public ICommand ChangeCommand
        {
            get
            {
                return Get(() => ChangeCommand, new RelayCommand(() =>
                {
                    var temp = new Connection(_srcConnection);
                    _srcConnection = new Connection(_dstConnection);
                    _dstConnection = new Connection(temp);
                }));
            }
        }

        public ICommand LoginCommand
        {
            get
            {
                return Get(() => LoginCommand, new RelayCommand(() =>
                {
                    var openWindow = new OpenWindow();
                    openWindow.ShowDialog();
                    if (openWindow.Tag is Tuple<Connection, Connection> result)
                        (_srcConnection, _dstConnection) = result;
                }));
            }
        }
    }
}
