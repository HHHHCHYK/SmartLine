// See https://aka.ms/new-console-template for more information

using Abstractions.Events;
using SmartLine;

Factory.LoadModule();

Console.WriteLine("Hello，Myself");

while (true)
{
    var read = Console.ReadLine();
    if (string.IsNullOrEmpty(read)) continue;
    if (read.ToLower() == "start")
    {
        MainEventBus.Instance.Publish(new DataCollectController(true));
        MainEventBus.Instance.Publish(new DataAnalyzerController(true));
        MainEventBus.Instance.Publish(LogEvent.Info("Start Input"));
        continue;
    }

    if (read.ToLower() == "stop")
    {
        MainEventBus.Instance.Publish(new DataCollectController(false));
        MainEventBus.Instance.Publish(new DataAnalyzerController(false));
        MainEventBus.Instance.Publish(LogEvent.Info("Stop Input"));
        continue;
    }

    MainEventBus.Instance.Publish(new LogEvent(LogLevel.Info, read));
}