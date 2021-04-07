using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace CloudSync.Events
{
    public class EventBus : IEventBus
    {
        private static volatile EventBus _instance;
        private static readonly object Lock = new object();
        private readonly object _dictLock = new object();
        private readonly Dictionary<Type, List<WeakReference>> _subscribers = new Dictionary<Type, List<WeakReference>>();

        public static EventBus Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (Lock)
                    {
                        if (_instance == null)
                            _instance = new EventBus();
                    }
                }

                return _instance;
            }
        }

        #region Private members
        private EventBus()
        {

        }

        private List<WeakReference> GetSubscribers(Type type)
        {
            List<WeakReference> subscribers;

            lock (Lock)
            {
                if (!_subscribers.TryGetValue(type, out subscribers))
                {
                    subscribers = new List<WeakReference>();
                    _subscribers.Add(type, subscribers);
                }
            }

            return subscribers;
        }

        private void Invoke<TEventType>(TEventType e, IEvent<TEventType> subscriber)
        {
            var syncContext = SynchronizationContext.Current ?? new SynchronizationContext();
            syncContext.Post(s => subscriber.HandleEvent(e), null);
        }
        #endregion

        public void RegisterHandler(object subscriber)
        {
            lock (_dictLock)
            {
                var types = subscriber.GetType().GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEvent<>));
                var weakRef = new WeakReference(subscriber);

                foreach (var type in types)
                {
                    var subscribers = GetSubscribers(type);
                    subscribers.Add(weakRef);
                }
            }
        }

        public void UnregisterHandler(object subscriber)
        {
            lock (_dictLock)
            {
                var types = subscriber.GetType().GetInterfaces().Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEvent<>));
                foreach (var type in types)
                {
                    var subscribers = GetSubscribers(type);
                    var removed = new List<WeakReference>();

                    foreach (var weakRef in subscribers)
                    {
                        if (weakRef.IsAlive)
                        {
                            if (weakRef.Target.Equals(subscriber))
                                removed.Add(weakRef);
                        }
                        else
                            removed.Add(weakRef);
                    }

                    if (removed.Any())
                        lock (_dictLock)
                        {
                            foreach (var remove in removed)
                                subscribers.Remove(remove);
                        }
                }
            }
        }

        public void Publish<TEventType>(TEventType e)
        {
            var type = typeof(IEvent<>).MakeGenericType(typeof(TEventType));
            var subscribers = GetSubscribers(type);
            var removed = new List<WeakReference>();

            foreach (var weakRef in subscribers)
            {
                if (weakRef.IsAlive)
                {
                    var subscriber = (IEvent<TEventType>)weakRef.Target;
                    Invoke(e, subscriber);
                }
                else
                    removed.Add(weakRef);
            }

            if (removed.Any())
            {
                lock (_dictLock)
                {
                    foreach (var remove in removed)
                        subscribers.Remove(remove);
                }
            }
        }
    }
}