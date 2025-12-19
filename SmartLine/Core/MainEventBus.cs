using Abstractions.Events;

namespace SmartLine;

public class MainEventBus : IEventBus
{
    public static MainEventBus Instance { get; } = new();

    private MainEventBus(){}
    private readonly Dictionary<Type, HashSet<Delegate>> _handlers = new();

    public void Subscribe<T>(Action<T> handler) where T : IEvent
    {
        Type tType = typeof(T);
        // 初始化Map的Key
        if (!_handlers.ContainsKey(tType))
        {
            _handlers.Add(tType, new());
        }
        _handlers[tType].Add(handler);  //类型擦除
    }


    public void Unsubscribe<T>(Action<T> handle) where T : IEvent
    {
        if(handle is not Action<object> action)return;
        Type tType = typeof(T);
        if(!_handlers.TryGetValue(tType, out var handler))return;
        handler.Remove(action);
    }

    public void Publish<T>(T @event) where T : IEvent
    {
        if(!_handlers.ContainsKey(@event.GetType()))return;
        foreach (var handler in _handlers[@event.GetType()])
        {
            if(handler is Action<T> action)action(@event);
        }
    }

    public void PublishByException(Exception? exception)
    {
        if(exception == null)return;
        Publish(new LogEvent(LogLevel.Error,exception.Message));
    }
}