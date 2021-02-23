namespace CloudSync.Events
{
    public interface IEventBus
    {
        void RegisterHandler(object subscriber);
        void UnregisterHandler(object subscriber);
        void Publish<TEventType>(TEventType e);
    }
}