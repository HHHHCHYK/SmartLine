using Abstractions;
using Abstractions.Attributes;
using Abstractions.Data;
using Abstractions.Events;

namespace TemperatureAlarmModule;

[Module("TemperatureAlarmModule")]
public class TemperatureAlarmModule : IModule, IDataAnalyzer
{
    public bool IsEnabled { get; private set; } = false;
    public string ModuleName => nameof(TemperatureAlarmModule);

    public TemperatureAlarmModule(IEventBus eventBus)
    {
        EventBus = eventBus;
        Initialize();
        EventBus.Publish(LogEvent.Info(nameof(TemperatureAlarmModule) + "：模块加载成功"));
    }

    private const float baseValue = 3f;
    private const float normalRateRange = 0.1f;

    private bool _isWithinNormalRange(float newValue)
    {
        float currentRate = (newValue - baseValue) / baseValue;
        if (Math.Abs(currentRate) > normalRateRange) return false;
        return true;
    }


    private void DataAnalyzer(CollectedData data)
    {
        if (!IsEnabled) return;
        if (!_isWithinNormalRange(data.Temperature))
        {
            EventBus.Publish(new LogEvent(LogLevel.Warning, ModuleName + "接收到异常数据！数据详情为：" + data.Temperature));
        }
        else EventBus.Publish(LogEvent.Info(nameof(TemperatureAlarmModule) + "模块接收到数据，结果：" + data.Temperature));
    }

    public void Initialize()
    {
        // 监听数据分析控制事件
        EventBus.Subscribe<DataAnalyzerController>(controller =>
        {
            if (controller.IsEnabled)
            {
                Start();
            }
            else
            {
                Stop();
            }
        });

        // 注册数据收集事件
        EventBus.Subscribe<DataCollectedEvent>(@event => DataAnalyzer(@event.Data));
    }


    public void Start()
    {
        IsEnabled = true;
    }

    public void Stop()
    {
        IsEnabled = false;
    }

    public IEventBus EventBus { get; }
}