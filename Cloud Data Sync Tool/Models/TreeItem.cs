using System.Collections.ObjectModel;
using System.ComponentModel;

namespace CloudSync.Models
{
    public class TreeItem : INotifyPropertyChanged
    {
        #region string Name
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        #endregion

        #region string ParentName
        private string _parentName;
        public string ParentName
        {
            get => _parentName;
            set
            {
                _parentName = value;
                OnPropertyChanged(nameof(ParentName));
            }
        }
        #endregion

        #region ObservableCollection<TreeItem> Children
        private ObservableCollection<TreeItem> _children;
        public ObservableCollection<TreeItem> Children
        {
            get => _children;
            set
            {
                _children = value;
                OnPropertyChanged(nameof(Children));
            }
        }
        #endregion

        public TreeItem()
        {
            Children = new ObservableCollection<TreeItem>();
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
