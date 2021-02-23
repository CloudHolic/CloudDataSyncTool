namespace CloudSync.Events
{
    public interface IEvent<in TEventType>
    {
        void HandleEvent(TEventType e);
    }
}