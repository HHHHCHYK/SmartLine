using Abstractions;
using Abstractions.Events;

namespace SimulateDataModule;

public class DataSender : IModule,IDataCollector
{
    public IEventBus EventBus { get; }
    
    
    public DataSender(IEventBus eventBus)
    {
        EventBus = eventBus;
    }
    
    public void Initialize()
    {
        EventBus.Subscribe<DataCollectControl>((startEvent) =>
        {
            if(startEvent.IsEnabled)Start();
            else Stop();
        });
        EventBus.Publish(new LogEvent(LogLevel.Info,nameof(SimulateDataModule)+" initialized"));
    }

    private CancellationTokenSource? _cts;
    
    public void Start()
    {
        _cts = new CancellationTokenSource();
        
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }

    private async Task StartDatSimulate(CancellationToken ct)
    {
        
    }

}