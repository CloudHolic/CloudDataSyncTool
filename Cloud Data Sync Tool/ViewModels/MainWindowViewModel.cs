using System;
using System.Collections.ObjectModel;
using System.Linq;
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
        private readonly string[] _systemSchemas = {"mysql", "sys", "information_schema", "performance_schema"};

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

        public object CurTable
        {
            get { return Get(() => CurTable); }
            set { Set(() => CurTable, value); }
        }

        public MainWindowViewModel(Tuple<Connection, Connection> cons)
        {
            (_srcConnection, _dstConnection) = cons;
            SrcTables = new ObservableCollection<TableList>();
            DstTables = new ObservableCollection<TableList>();

            LoadTables();
        }

        public ICommand CopyCommand
        {
            get
            {
                return Get(() => CopyCommand, new RelayCommand(() =>
                {
                    var temp = (string)CurTable;
                    
                    // TODO: Find CurTable's parent and copy table(s) to Dst.

                    LoadTables();
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

                    LoadTables();
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
                    {
                        (_srcConnection, _dstConnection) = result;
                        LoadTables();
                    }
                }));
            }
        }

        private void LoadTables()
        {
            var dbUtil = new DbUtils(_srcConnection, _dstConnection);
            SrcName = _srcConnection.Name;
            DstName = _dstConnection.Name;

            var srcSchemas = dbUtil.FindSchemas();
            var dstSchemas = dbUtil.FindSchemas(false);

            foreach (var schema in srcSchemas.Where(schema => !_systemSchemas.Contains(schema)))
            {
                SrcTables.Add(new TableList
                {
                    SchemaName = schema,
                    TableNames = new ObservableCollection<string>(dbUtil.FindTables(schema))
                });
            }

            foreach (var schema in dstSchemas.Where(schema => !_systemSchemas.Contains(schema)))
            {
                DstTables.Add(new TableList
                {
                    SchemaName = schema,
                    TableNames = new ObservableCollection<string>(dbUtil.FindTables(schema, false))
                });
            }
        }
    }
}
