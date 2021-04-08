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

        public string NotifyText
        {
            get { return Get(() => NotifyText); }
            set { Set(() => NotifyText, value); }
        }

        public NotifyViewModel(string title, string notifyText)
        {
            Title = title;
            NotifyText = notifyText;
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
