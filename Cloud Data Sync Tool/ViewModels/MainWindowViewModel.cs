using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using MahApps.Metro.Controls.Dialogs;

namespace CloudSync.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private Connection _srcConnection, _dstConnection;
        private readonly StackPanel _srcPanel, _dstPanel;
        private readonly ICustomDialogManager _dialogManager;
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

        public bool IsWorking
        {
            get { return Get(() => IsWorking); }
            set { Set(() => IsWorking, value); }
        }

        public int ProgressValue
        {
            get { return Get(() => ProgressValue); }
            set { Set(() => ProgressValue, value); }
        }

        public string ProgressStatus
        {
            get { return Get(() => ProgressStatus); }
            set { Set(() => ProgressStatus, value); }
        }

        public MainWindowViewModel(Tuple<Connection, Connection> cons, Tuple<StackPanel, StackPanel> panels)
        {
            (_srcConnection, _dstConnection) = cons;
            (_srcPanel, _dstPanel) = panels;
            IsSrcOpened = IsDstOpened = false;
            IsWorking = false;

            ProgressValue = 0;
            ProgressStatus = "Ready";

            _dialogManager = new CustomDialogManager(new MetroDialogSettings
            {
                AnimateHide = false,
                AnimateShow = false
            });

            BulkCopyWorker.Instance.InitWorker(CopyCompleted, ProgressChanged);

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
                    if (IsWorking)
                    {
                        BulkCopyWorker.Instance.CancelWorker();
                        return;
                    }

                    IsWorking = true;

                    string srcSchema = "", dstSchema = "";

                    var tables = new List<string>();
                    foreach (var child in _srcPanel.Children)
                    {
                        var schemaViewModel = (SchemaEntryViewModel)((SchemaEntry)child).DataContext;
                        if (schemaViewModel.IsChecked)
                        {
                            srcSchema = schemaViewModel.SchemaName;
                            var tableList = ((DependencyObject)child).FindChild<ListBox>();
                            if (tableList != null && tableList.SelectedItems.Count > 0)
                                tables.AddRange(tableList.SelectedItems.Cast<string>());
                        }
                    }

                    foreach (var child in _dstPanel.Children)
                    {
                        var schemaViewModel = (SchemaEntryViewModel)((SchemaEntry)child).DataContext;
                        if (schemaViewModel.IsChecked)
                            dstSchema = schemaViewModel.SchemaName;
                    }
                    
                    BulkCopyWorker.Instance.StartWorker(new WorkerArgs
                    {
                        SrcConnection = _srcConnection,
                        DstConnection = _dstConnection,
                        SrcSchemaName = srcSchema,
                        DstSchemaName = dstSchema,
                        DumpDirectory = @"D:\DbDumps",
                        SrcTables = tables,
                        DeleteFile = true
                    });
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
                    
                    ProgressValue = 0;
                    ProgressStatus = "Ready";
                }, () => !IsWorking));
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

                        ProgressValue = 0;
                        ProgressStatus = "Ready";
                    }
                }, () => !IsWorking));
            }
        }

        private void CopyCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
                ProgressStatus = "Cancelled. - " + ProgressStatus;
            else if (e.Error != null)
                ProgressStatus = "Error occurred. - " + e.Error.Message;
            else
                ProgressStatus = $"Completed. - {(int) e.Result} tables copied.";
            
            _dialogManager.ShowMessageBox("Copy Completed", ProgressStatus);

            IsWorking = false;
            LoadTables();
        }

        private void ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressValue = e.ProgressPercentage;
            ProgressStatus = (string) e.UserState;
        }

        private void LoadTables()
        {
            IsSrcChecked = IsDstChecked = false;
            IsSrcOpened = IsDstOpened = false;

            _srcPanel.Children.Clear();
            _dstPanel.Children.Clear();

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
