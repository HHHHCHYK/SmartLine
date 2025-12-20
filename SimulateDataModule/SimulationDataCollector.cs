using System.Diagnostics.CodeAnalysis;
using Abstractions;
using Abstractions.Attributes;
using Abstractions.Data;
using Abstractions.Events;

namespace SimulateDataModule;

[Module("SimulationDataCollectorModule")]
[SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
public class SimulationDataCollector : IModule, IDataCollector
{
    public IEventBus EventBus { get; }
    private static readonly Random Random = new Random();


    /// <summary>
    /// 基准值
    /// </summary>
    public float BaseData { get; } = 3f;

    /// <summary>
    /// 正常变化率
    /// </summary>
    public float NormalRateRange = 0.1f;

    /// <summary>
    /// 错误时变化率
    /// </summary>
    public float ErrorRateRange = 0.5f;

    /// <summary>
    /// 错误发生率
    /// </summary>
    public float ErrorRate = 0.1f;


    public SimulationDataCollector(IEventBus eventBus)
    {
        EventBus = eventBus;
    }

    public void Initialize()
    {
        EventBus.Subscribe<DataCollectControl>((startEvent) =>
        {
            if (startEvent.IsEnabled) Start();
            else Stop();
        });
        EventBus.Publish(new LogEvent(LogLevel.Info, nameof(SimulateDataModule) + " initialized"));
    }

    private CancellationTokenSource? _cts;
    private Task? _simulateDataTask;

    public void Start()
    {
        _cts = new CancellationTokenSource();
        _simulateDataTask = StartDatSimulate(_cts.Token);
    }

    public void Stop()
    {
        _cts?.Cancel();
    }

    private async Task StartDatSimulate(CancellationToken ct)
    {
        while (ct.IsCancellationRequested == false)
        {
            // 程序生成模拟值
            float data = SpawnSimulateDataOnce();
            CollectedData cd = new()
            {
                DeviceId = DateTime.Now.Ticks
                    .GetHashCode().ToString(),
                Temperature = data
            };
            EventBus.Publish(new DataCollectedEvent(cd));
            await Task.Delay(1000, ct);
        }
    }

    private float SpawnSimulateDataOnce()
    {
        double randomDouble = Random.NextDouble();
        if (randomDouble > ErrorRate) //随机到正常数值
        {
            float realData = (float)(Random.NextDouble() * NormalRateRange * BaseData);
            return realData;
        }
        else
        {
            return (float)(Random.NextDouble() * ErrorRateRange * BaseData);
        }
    }
}