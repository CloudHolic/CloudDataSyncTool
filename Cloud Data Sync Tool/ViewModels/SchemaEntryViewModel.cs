using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using CloudSync.Behaviors;
using CloudSync.Commands;
using CloudSync.Controls;
using CloudSync.Events;
using CloudSync.Models;
using MahApps.Metro.Controls;
using Microsoft.Xaml.Behaviors;
using Xceed.Wpf.AvalonDock.Controls;

namespace CloudSync.ViewModels
{
    public class SchemaEntryViewModel : ViewModelBase, IEvent<SearchTextChangedEvent>
    {
        public bool IsChecked
        {
            get { return Get(() => IsChecked); }
            set { Set(() => IsChecked, value); }
        }

        public bool IsOpened
        {
            get { return Get(() => IsOpened); }
            set { Set(() => IsOpened, value); }
        }

        public string SchemaName
        {
            get { return Get(() => SchemaName); }
            set { Set(() => SchemaName, value); }
        }

        public ObservableCollection<string> Tables
        {
            get { return Get(() => Tables); }
            set { Set(() => Tables, value); }
        }

        public CollectionViewSource TablesViewSource
        {
            get { return Get(() => TablesViewSource); }
            set { Set(() => TablesViewSource, value); }
        }

        private readonly bool _isSrc;

        public SchemaEntryViewModel(TableList tables, bool isSrc)
        {
            IsOpened = false;
            _isSrc = isSrc;
            SchemaName = tables.SchemaName;
            Tables = new ObservableCollection<string>(tables.Tables);
            TablesViewSource = new CollectionViewSource {Source = Tables};

            EventBus.Instance.RegisterHandler(this);
        }

        public ICommand DoubleClickCommand
        {
            get
            {
                return Get(() => DoubleClickCommand, new RelayCommand(() =>
                {
                    IsOpened = !IsOpened;
                }));
            }
        }

        public ICommand GotFocusCommand
        {
            get
            {
                return Get(() => GotFocusCommand, new RelayCommand<SchemaEntry>(schema =>
                {
                    if (!OperatingSystem.IsWindowsVersionAtLeast(7))
                        return;

                    var parent = schema.TryFindParent<StackPanel>();
                    var children = parent.FindVisualChildren<SchemaEntry>();
                    
                    foreach (var child in children)
                    {
                        var button = child.FindChild<Button>();

                        if (child == schema)
                            ((SchemaButtonBehavior)Interaction.GetBehaviors(button)[0]).IsChecked = true;
                        else
                        {
                            ((SchemaButtonBehavior)Interaction.GetBehaviors(button)[0]).IsChecked = false;

                            var list = child.FindVisualChildren<ListBoxItem>();
                            foreach (var item in list)
                                item.IsSelected = false;
                        }
                    }
                }));
            }
        }

        public void HandleEvent(SearchTextChangedEvent e)
        {
            if (_isSrc != e.IsSrc)
                return;

            if (string.IsNullOrWhiteSpace(e.SearchText))
                TablesViewSource.View.Filter = null;
            else
                TablesViewSource.View.Filter = de => de.ToString().Contains(e.SearchText);
        }
    }
}
