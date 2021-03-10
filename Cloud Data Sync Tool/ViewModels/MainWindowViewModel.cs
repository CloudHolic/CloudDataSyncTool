using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CloudSync.Commands;
using CloudSync.Contents;
using CloudSync.Controls;
using CloudSync.Models;
using CloudSync.Utils;
using MahApps.Metro.Controls;

namespace CloudSync.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Connection _srcConnection, _dstConnection;
        private readonly StackPanel _srcPanel, _dstPanel;
        private readonly string[] _systemSchemas = {"mysql", "sys", "information_schema", "performance_schema"};

        public bool IsSrcOpened
        {
            get { return Get(() => IsSrcOpened); }
            set { Set(() => IsSrcOpened, value); }
        }

        public bool IsSrcChecked
        {
            get { return Get(() => IsSrcChecked); }
            set { Set(() => IsSrcChecked, value); }
        }

        public string SrcDb
        {
            get { return Get(() => SrcDb); }
            set { Set(() => SrcDb, value); }
        }

        public bool IsDstOpened
        {
            get { return Get(() => IsDstOpened); }
            set { Set(() => IsDstOpened, value); }
        }

        public bool IsDstChecked
        {
            get { return Get(() => IsDstChecked); }
            set { Set(() => IsDstChecked, value); }
        }

        public string DstDb
        {
            get { return Get(() => DstDb); }
            set { Set(() => DstDb, value); }
        }

        public MainWindowViewModel(Tuple<Connection, Connection> cons, Tuple<StackPanel, StackPanel> panels)
        {
            (_srcConnection, _dstConnection) = cons;
            (_srcPanel, _dstPanel) = panels;
            IsSrcOpened = IsDstOpened = false;

            LoadTables();
        }

        public ICommand SrcDoubleClickCommand
        {
            get
            {
                return Get(() => SrcDoubleClickCommand, new RelayCommand(() =>
                {
                    IsSrcOpened = !IsSrcOpened;
                }));
            }
        }

        public ICommand DstDoubleClickCommand
        {
            get
            {
                return Get(() => DstDoubleClickCommand, new RelayCommand(() =>
                {
                    IsDstOpened = !IsDstOpened;
                }));
            }
        }

        public ICommand CopyCommand
        {
            get
            {
                return Get(() => CopyCommand, new RelayCommand(() =>
                {
                    //var dbUtil = new DbUtils(_srcConnection, _dstConnection);
                    //var tablePath = $"{CurTable.ParentName}.{CurTable.Name}";

                    var items = new List<string>();
                    foreach (var child in _srcPanel.Children)
                    {
                        var tableList = ((DependencyObject)child).FindChild<ListBox>();

                        if (tableList != null)
                            items.AddRange(tableList.SelectedItems.Cast<string>());
                    }
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
            _srcPanel.Children.RemoveRange(1, _srcPanel.Children.Count - 1);
            _dstPanel.Children.RemoveRange(1, _dstPanel.Children.Count - 1);

            var dbUtil = new DbUtils(_srcConnection, _dstConnection);
            
            SrcDb = _srcConnection.Name;
            DstDb = _dstConnection.Name;

            var srcSchemas = dbUtil.FindSchemas();
            var dstSchemas = dbUtil.FindSchemas(false);

            foreach (var schema in srcSchemas.Where(schema => !_systemSchemas.Contains(schema)))
            {
                var srcList = new TableList {SchemaName = schema};
                foreach(var table in dbUtil.FindTables(schema))
                    srcList.Tables.Add(table);
                _srcPanel.Children.Add(new SchemaEntry(srcList));
            }

            foreach (var schema in dstSchemas.Where(schema => !_systemSchemas.Contains(schema)))
            {
                var dstList = new TableList {SchemaName = schema};
                foreach (var table in dbUtil.FindTables(schema))
                    dstList.Tables.Add(table);
                _dstPanel.Children.Add(new SchemaEntry(dstList, false));
            }
        }
    }
}
