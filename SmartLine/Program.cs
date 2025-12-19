// See https://aka.ms/new-console-template for more information

using Abstractions.Events;
using SmartLine;

Factory.LoadModule();

Console.WriteLine("Hello，Myself");

while (true)
{
    var read = Console.ReadLine();
    if(string.IsNullOrEmpty(read))continue;
    MainEventBus.Instance.Publish(new LogEvent(LogLevel.Info,read));
}
