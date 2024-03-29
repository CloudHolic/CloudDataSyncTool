﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using CloudSync.Commands;
using CloudSync.Contents;
using CloudSync.Controls;
using CloudSync.Events;
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
        private readonly DispatcherTimer _timer;
        private readonly Stopwatch _stopwatch;
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

        public string Time
        {
            get { return Get(() => Time); }
            set { Set(() => Time, value); }
        }

        public string SearchText
        {
            get { return Get(() => SearchText); }
            set { Set(() => SearchText, value); }
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

            _timer = new DispatcherTimer {Interval = TimeSpan.FromSeconds(0.01)};
            _timer.Tick += TimerTick;

            _stopwatch = new Stopwatch();

            BulkCopyWorker.Instance.InitWorker(CopyCompleted, ProgressChanged);

            LoadTables();
        }

        public ICommand SrcSearchTextChangedCommand
        {
            get
            {
                return Get(() => SrcSearchTextChangedCommand, new RelayCommand(() =>
                {
                    EventBus.Instance.Publish(new SearchTextChangedEvent
                    {
                        IsSrc = true,
                        SearchText = SearchText
                    });
                }));
            }
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
                    _stopwatch.Start();
                    _timer.Start();

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

        public ICommand RefreshCommand
        {
            get
            {
                return Get(() => RefreshCommand, new RelayCommand(() =>
                {
                    LoadTables();

                    ProgressValue = 0;
                    ProgressStatus = "Ready";
                }, () => !IsWorking));
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

        public ICommand ClosingCommand 
        {
            get
            {
                return Get(() => ClosingCommand, new RelayCommand<MetroWindow>(window =>
                {
                    ConfigManager.Instance.Config.WindowLeft = window.Left;
                    ConfigManager.Instance.Config.WindowTop = window.Top;
                    ConfigManager.Instance.Config.WindowHeight = window.ActualHeight;
                    ConfigManager.Instance.Config.WindowWidth = window.ActualWidth;

                    ConfigManager.Instance.Save();
                }));
            }
        }

        private void TimerTick(object sender, EventArgs e)
        {
            if (_stopwatch.IsRunning)
            {
                var elapsed = _stopwatch.Elapsed;
                Time = $"Elapsed Time: {elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}.{elapsed.Milliseconds / 10:D2}";
            }
        }

        private void CopyCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string title, notifyText;

            if (LastErrorManager.Instance.CheckError())
            {
                title = "Copy failed";
                notifyText = ProgressStatus + "\n" + LastErrorManager.Instance.GetLastError();
                ProgressStatus = "Error occurred. - " + ProgressStatus;
            }
            else
            {
                title = "Copy Completed";

                if (e.Cancelled)
                {
                    notifyText = ProgressStatus;
                    ProgressStatus = "Cancelled. - " + notifyText;
                }
                else
                {
                    notifyText = $"{(int) e.Result} tables copied.";
                    ProgressStatus = "Completed. - " + notifyText;
                }
            }
            
            _dialogManager.ShowDialogAsync(new NotifyView(title, notifyText));
            IsWorking = false;
            LoadTables();
            EventBus.Instance.Publish(new SearchTextChangedEvent
            {
                IsSrc = true,
                SearchText = SearchText
            });

            _stopwatch.Reset();
            _timer.Stop();
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
                _srcPanel.Children.Add(new SchemaEntry(srcList, true));
            }

            foreach (var schema in dstSchemas.Where(schema => !_systemSchemas.Contains(schema)))
            {
                var dstList = new TableList {SchemaName = schema};
                foreach (var table in dbUtil.FindTables(schema, false))
                    dstList.Tables.Add(table);
                _dstPanel.Children.Add(new SchemaEntry(dstList, false, false));
            }
        }
    }
}
