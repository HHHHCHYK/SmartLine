namespace Abstractions.Events;

public interface IEventBus
{
    public void Subscribe<T>(Action<T> handler) where T : IEvent;
    public void Unsubscribe<T>(Action<T> handle) where T : IEvent;
    public void Publish<T>(T @event) where T : IEvent;
}