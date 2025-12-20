using System;
using Abstractions.Data;

namespace Abstractions.Events;

public interface IEvent
{
}

/// <summary>
/// 发布数据收集开始数据
/// </summary>
// ReSharper disable once ClassNeverInstantiated.Global
public class DataCollectController(bool enable) : IEvent
{
    public bool IsEnabled { get; } = enable;
}

public class DataAnalyzerController(bool enable) : IEvent
{
    public bool IsEnabled { get; } = enable;
}

public class DataCollectedEvent(CollectedData data) : IEvent
{
    public CollectedData Data { get; } = data;
}

public class AnalysisResultEvent(CollectedData data) : IEvent
{
    public CollectedData Data { get; } = data;
}

public class WarningEvent(string message) : IEvent
{
    public string Message { get; } = message;
}

public class ErrorEvent(string message) : IEvent
{
    public string Message { get; } = message;
}

public class MessagePublishEvent : IEvent;

public class LogEvent(LogLevel logLevel, string message) : IEvent
{
    // 事件参数
    private readonly DateTime _createdTimeStamp = DateTime.Now; //事件产生时间戳   

    // ReSharper disable once MemberCanBePrivate.Global
    public string Message => message; //事件描述
    private LogLevel LogLevel => logLevel; //严重等级

    public override string ToString()
    {
        return $"[{LogLevel}]: {Message} -----{_createdTimeStamp}";
    }

    public static LogEvent Info(string message)
    {
        return new LogEvent(LogLevel.Info, message);
    }
}