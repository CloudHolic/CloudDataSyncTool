using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

// ReSharper disable InvertIf
namespace CloudSync.ViewModels
{
    public abstract class VerifiableViewModelBase : ViewModelBase, IDataErrorInfo
    {
        private readonly ConcurrentDictionary<string, Binder> _ruleMap = new ConcurrentDictionary<string, Binder>();

        public bool HasErrors
        {
            get
            {
                var values = _ruleMap.Values.ToList();
                return values.Any(b => b.HasError);
            }
        }

        #region IDataErrorInfo Members
        public string this[string propertyName]
        {
            get
            {
                if (_ruleMap.ContainsKey(propertyName))
                {
                    var message = _ruleMap[propertyName].Error;
                    if (message != null)
                        return message;
                }

                return string.Empty;
            }
        }

        public string Error
        {
            get
            {
                var values = _ruleMap.Values.ToList();
                var binder = values.FirstOrDefault(b => b.HasError);
                return binder != null ? binder.Error : string.Empty;
            }
        }
        #endregion

        #region Binder
        private class Binder
        {
            private readonly string _propertyName;
            private readonly Dictionary<Func<bool>, string> _rules;

            internal string Error { get; private set; }
            internal bool HasError { get; private set; }
            internal bool IsDirty { private get; set; }

            internal Binder(string propertyName)
            {
                _propertyName = propertyName;
                _rules = new Dictionary<Func<bool>, string>();
                IsDirty = true;
            }

            internal void AddRule(Func<bool> rule, string message)
            {
                if (_rules.ContainsKey(rule))
                    _rules[rule] = message;
                else
                    _rules.Add(rule, message);

                IsDirty = true;
            }

            internal void Update()
            {
                if (IsDirty)
                {
                    try
                    {
                        var hasError = false;
                        foreach (var rule in _rules)
                        {
                            var result = rule.Key();
                            if (!result)
                            {
                                Error = rule.Value;
                                HasError = hasError = true;
                                break;
                            }
                        }

                        if (!hasError)
                        {
                            Error = null;
                            HasError = false;
                        }
                    }
                    catch (Exception e)
                    {
                        Error = e.GetBaseException().Message;
                        HasError = true;
                    }
                }

                IsDirty = true;
            }
        }
        #endregion

        protected override void Set<T>(Expression<Func<T>> path, T value, bool forceUpdate)
        {
            base.Set(path, value, forceUpdate);
            var propertyName = GetPropertyName(path);
            if (_ruleMap.ContainsKey(propertyName))
            {
                _ruleMap[propertyName].IsDirty = true;
                _ruleMap[propertyName].Update();
            }
        }

        public void AddRule<T>(Expression<Func<T>> expression, Func<bool> rule, string message)
        {
            var propertyName = GetPropertyName(expression);
            if (_ruleMap.ContainsKey(propertyName))
                _ruleMap[propertyName].AddRule(rule, message);
            else
            {
                var binder = new Binder(propertyName);
                binder.AddRule(rule, message);
                _ruleMap.TryAdd(propertyName, binder);
            }
        }

        public void Verify()
        {
            var values = _ruleMap.Values.ToList();
            values.ForEach(b => b.Update());
        }

        public void Verify(bool isForceUpdate)
        {
            var values = _ruleMap.Values.ToList();
            values.ForEach(b =>
            {
                b.IsDirty = isForceUpdate;
                b.Update();
            });
        }
    }
}