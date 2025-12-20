using Abstractions;
using Abstractions.Attributes;
using Abstractions.Events;

namespace TemperatureAlarmModule;

[Module("TemperatureAlarmModule")]
public class TemperatureAlarmModule : IModule ,IDataAnalyzer
{
    public TemperatureAlarmModule(IEventBus eventBus)
    {
        EventBus = eventBus;
    }
    
    
    public void Initialize()
    {
        throw new NotImplementedException();
    }

    public void Start()
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }
    
    public IEventBus EventBus { get; }
}