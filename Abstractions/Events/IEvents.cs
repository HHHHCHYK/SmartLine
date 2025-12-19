namespace Abstractions.Events;

public interface IEvent
{
}

public class DataCollectedEvent : IEvent
{
}

public class AnalysisResultEvent : IEvent
{
}

public class WarningEvent : IEvent
{
}

public class MessagePublishEvent : IEvent
{
}

public class LogEvent(LogLevel logLevel, string message) : IEvent
{

    // 事件参数
    public DateTime CreatedTimeStamp = DateTime.Now;    //事件产生时间戳   
    public string Message => message;                   //事件描述
    public LogLevel LogLevel => logLevel;               //严重等级
    public override string ToString()
    {
        return $"[{LogLevel}]: {Message} -----{CreatedTimeStamp}";
    }
}