using System;
using System.Threading.Tasks;

namespace CloudSync.ViewModels
{
    public abstract class DialogViewModelBase : ViewModelBase, IDialogViewModelBase
    {
        public event EventHandler Closed;
        public Task Task => _tcs.Task;

        private readonly TaskCompletionSource<int> _tcs;

        protected DialogViewModelBase()
        {
            _tcs = new TaskCompletionSource<int>();
        }

        protected void Close()
        {
            _tcs.SetResult(0);

            Closed?.Invoke(this, EventArgs.Empty);
        }
    }

    public abstract class DialogViewModelBase<TResult> : ViewModelBase, IDialogViewModelBase
    {
        public event EventHandler Closed;
        public Task<TResult> Task => _tcs.Task;

        private readonly TaskCompletionSource<TResult> _tcs;

        protected DialogViewModelBase()
        {
            _tcs = new TaskCompletionSource<TResult>();
        }

        protected void Close(TResult result)
        {
            _tcs.SetResult(result);

            Closed?.Invoke(this, EventArgs.Empty);
        }
    }
}