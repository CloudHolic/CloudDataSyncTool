using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;

namespace CloudSync.Utils
{
    public interface ICustomDialogManager
    {
        Task ShowDialogAsync(CustomDialog view);

        Task ShowDialogAsync<TView>() where TView : CustomDialog;

        Task<TResult> ShowDialogAsync<TResult>(CustomDialog view);

        Task<TResult> ShowDialogAsync<TView, TResult>() where TView : CustomDialog;

        Task<MessageDialogResult> ShowMessageBox(string title, string message,
            MessageDialogStyle style = MessageDialogStyle.Affirmative, MetroDialogSettings settings = null);
    }
}