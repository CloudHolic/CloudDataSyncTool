using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using CloudSync.Commands;
using CloudSync.Models;

namespace CloudSync.ViewModels
{
    public class SchemaEntryViewModel : ViewModelBase
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

        public ObservableCollection<string> SelectedTables
        {
            get { return Get(() => SelectedTables); }
            set { Set(() => SelectedTables, value); }
        }

        public SchemaEntryViewModel(TableList tables)
        {
            IsOpened = false;
            SchemaName = tables.SchemaName;
            Tables = new ObservableCollection<string>(tables.Tables);
            SelectedTables = new ObservableCollection<string>();
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

        public ICommand LostFocusCommand
        {
            get
            {
                return Get(() => LostFocusCommand, new RelayCommand(() =>
                {
                    SelectedTables.Clear();
                }));
            }
        }
    }
}
