using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;

namespace CloudSync.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly ConcurrentDictionary<string, object> _propertyValueMap;

        protected ViewModelBase()
        {
            _propertyValueMap = new ConcurrentDictionary<string, object>();
        }

        protected static string GetPropertyName<T>(Expression<Func<T>> expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));

            var body = expression.Body;
            if (!(body is MemberExpression memberExpression))
                memberExpression = (MemberExpression)((UnaryExpression)body).Operand;

            return memberExpression.Member.Name;
        }

        protected virtual void OnPropertyChanged<T>(Expression<Func<T>> path)
        {
            var propertyName = GetPropertyName(path);
            InternalPropertyUpdate(propertyName);
        }

        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected T Get<T>(Expression<Func<T>> path)
        {
            return Get(path, default(T));
        }

        protected virtual T Get<T>(Expression<Func<T>> path, T defaultValue)
        {
            var propertyName = GetPropertyName(path);
            if (_propertyValueMap.ContainsKey(propertyName))
                return (T)_propertyValueMap[propertyName];

            _propertyValueMap.TryAdd(propertyName, defaultValue);
            return defaultValue;
        }

        protected virtual void Set<T>(Expression<Func<T>> path, T value)
        {
            Set(path, value, false);
        }

        protected virtual void Set<T>(Expression<Func<T>> path, T value, bool forceUpdate)
        {
            var oldValue = Get(path);
            var propertyName = GetPropertyName(path);

            if (!Equals(value, oldValue) || forceUpdate)
            {
                _propertyValueMap[propertyName] = value;
                OnPropertyChanged(path);
            }
        }

        private void InternalPropertyUpdate(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}