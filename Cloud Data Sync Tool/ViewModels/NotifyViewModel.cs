using System.Windows.Input;
using CloudSync.Commands;
using CloudSync.Models;

namespace CloudSync.ViewModels
{
    public class NotifyViewModel : DialogViewModelBase
    {
        public string Title
        {
            get { return Get(() => Title); }
            set { Set(() => Title, value); }
        }

        public NotifyViewModel(string title, Error error)
        {
            Title = title;
        }

        public ICommand CloseCommand
        {
            get
            {
                return Get(() => CloseCommand, new RelayCommand(Close));
            }
        }
    }
}
