using Abstractions;
using Abstractions.Attributes;
using Abstractions.Events;

namespace ConsoleAndLogModule;

[Module("ConsoleAndLogModule")]
public class ConsoleAndLogModule : IModule, INotifier
{
    public ConsoleAndLogModule(IEventBus eventBus)
    {
        EventBus = eventBus;
        Initialize();
    }
    public IEventBus EventBus { get; }

    public string ModuleName => nameof(ConsoleAndLogModule);

    public void Initialize()
    {
        Start();    //Debug：初始化后立刻启动
        EventBus.Publish(LogEvent.Info(nameof(ConsoleAndLogModule) + "：模块加载成功"));
    }

    public void Start()
    {
        EventBus.Subscribe<LogEvent>(LogEventFunc);
    }

    public void Stop()
    {
        EventBus.Unsubscribe<LogEvent>(LogEventFunc);
        EventBus.Publish(new LogEvent(LogLevel.Info,$"{nameof(ConsoleAndLogModule)} 停止"));
    }

    /// <summary>
    /// 接受日志事件并处理
    /// </summary>
    /// <param name="logEvent">日志事件，通常由EventBus传递</param>
    private void LogEventFunc(LogEvent logEvent)
    {
        Console.WriteLine(logEvent);
    }

}