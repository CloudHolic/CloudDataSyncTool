using System;

namespace CloudSync.ViewModels
{
    public interface IDialogViewModelBase
    {
        event EventHandler Closed;
    }
}