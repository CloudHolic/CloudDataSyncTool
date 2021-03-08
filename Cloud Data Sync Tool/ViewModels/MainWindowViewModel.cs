using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using CloudSync.Commands;
using CloudSync.Contents;
using CloudSync.Controls;
using CloudSync.Models;
using CloudSync.Utils;

namespace CloudSync.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Connection _srcConnection, _dstConnection;
        private StackPanel _srcPanel;
        private readonly string[] _systemSchemas = {"mysql", "sys", "information_schema", "performance_schema"};

        public string SrcDb
        {
            get { return Get(() => SrcDb); }
            set { Set(() => SrcDb, value); }
        }

        public ObservableCollection<TreeItem> SrcTable
        {
            get { return Get(() => SrcTable); }
            set { Set(() => SrcTable, value); }
        }

        public ObservableCollection<TreeItem> DstTable
        {
            get { return Get(() => DstTable); }
            set { Set(() => DstTable, value); }
        }

        public TreeItem CurTable
        {
            get { return Get(() => CurTable); }
            set { Set(() => CurTable, value); }
        }

        public MainWindowViewModel(Tuple<Connection, Connection> cons, StackPanel srcPanel)
        {
            (_srcConnection, _dstConnection) = cons;
            SrcTable = new ObservableCollection<TreeItem>();
            DstTable = new ObservableCollection<TreeItem>();

            _srcPanel = srcPanel;

            LoadTables();
        }

        public ICommand CopyCommand
        {
            get
            {
                return Get(() => CopyCommand, new RelayCommand(() =>
                {
                    var dbUtil = new DbUtils(_srcConnection, _dstConnection);
                    var tablePath = $"{CurTable.ParentName}.{CurTable.Name}";
                    /*
                    var tables = dbUtil.FindTables(srcSchema);
                    var files = tables.Select(table => $@"D:\DbDumps\{srcSchema}-{table}.csv").ToList();
                    var tableList = TableName.GetTableNameList(tables, files);

                    if (tableList != null && tableList.Count > 0)
                    {
                        dbUtil.SaveTables(srcSchema, tableList);
                        dbUtil.BulkLoad(srcSchema, dstSchema, tableList, false);
                    }
                    */
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
            SrcTable.Clear();
            DstTable.Clear();

            var dbUtil = new DbUtils(_srcConnection, _dstConnection);
            //var src = new TreeItem();
            var dst = new TreeItem();

            //src.Name = _srcConnection.Name;
            SrcDb = _srcConnection.Name;
            dst.Name = _dstConnection.Name;

            var srcSchemas = dbUtil.FindSchemas();
            var dstSchemas = dbUtil.FindSchemas(false);

            foreach (var schema in srcSchemas.Where(schema => !_systemSchemas.Contains(schema)))
            {
                //var srcList = new TreeItem {Name = schema, ParentName = src.Name};
                var srcList = new TableList {SchemaName = schema};
                foreach(var table in dbUtil.FindTables(schema))
                    //srcList.Children.Add(new TreeItem {Name = table, ParentName = schema});
                    srcList.Tables.Add(table);
                //src.Children.Add(srcList);
                _srcPanel.Children.Add(new SchemaEntry(srcList));
            }
            //SrcTable.Add(src);

            foreach (var schema in dstSchemas.Where(schema => !_systemSchemas.Contains(schema)))
            {
                var dstList = new TreeItem {Name = schema, ParentName = dst.Name};
                foreach (var table in dbUtil.FindTables(schema))
                    dstList.Children.Add(new TreeItem {Name = table, ParentName = schema});
                dst.Children.Add(dstList);
            }
            DstTable.Add(dst);
        }
    }
}
