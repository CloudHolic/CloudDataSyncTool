using System;

namespace CloudSync.Commands
{
    public class RelayCommand : RelayCommandBase
    {
        public RelayCommand(Action execute) : this(execute, () => true)
        {
        }

        public RelayCommand(Action execute, Func<bool> canExecute) : base(o => execute(), o => canExecute())
        {
        }

        public bool CanExecute()
        {
            return CanExecute(null);
        }

        public void Execute()
        {
            Execute(null);
        }
    }

    public class RelayCommand<T> : RelayCommandBase
    {
        public RelayCommand(Action<T> execute) : this(execute, o => true)
        {
        }

        public RelayCommand(Action<T> execute, Func<T, bool> canExecute) : base(o => execute((T)o), o => canExecute((T)o))
        {
            var type = typeof(T);
            if (type.IsValueType && (!type.IsGenericType || !typeof(Nullable<>).IsAssignableFrom(type.GetGenericTypeDefinition())))
                throw new InvalidCastException("T for RelayCommand<T> is not an object nor Nullable.");
        }

        public bool CanExecute(T parameter)
        {
            return CanExecute((object)parameter);
        }

        public void Execute(T parameter)
        {
            Execute((object)parameter);
        }
    }
}