using Abstractions.Events;

namespace Abstractions;

public interface IModule
{
    string ModuleName { get; }
    void Initialize();
    void Start();
    void Stop();
}


/// <summary>
/// 数据采集模块
/// </summary>
public interface IDataCollector
{
    IEventBus EventBus { get; }
    
}

/// <summary>
/// 数据分析模块
/// </summary>
public interface IDataAnalyzer
{
    IEventBus EventBus { get; }
    
}

/// <summary>
/// 预警模块
/// </summary>
public interface IAlertEngine
{
    IEventBus EventBus { get; }
}

/// <summary>
/// 通知模块
/// </summary>
public interface INotifier
{
    IEventBus EventBus { get; }
    
}