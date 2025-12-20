using Abstractions.Events;

namespace TemperatureAlarmModule;

internal class TemperatureDataBuffer
{
    private static readonly int BufferSize = 1024;
    private readonly Queue<float> _dataBuffer = new();

    public void DataInput(IEventBus bus, float newValue)
    {
        // 注入数据
        _dataBuffer.Enqueue(newValue);
        
        // 清理缓冲区
        if (_dataBuffer.Count > BufferSize)
        {
            _dataBuffer.Dequeue();
        }
        
        //分析数据
        float averageGap = float.MaxValue;
        float lastValue = -300;
        foreach (var data in _dataBuffer)
        {
            // 如果还未赋值
            if (lastValue <= -300)
            {
                lastValue = data;
                continue;
            }
            
            float gap =  lastValue - data;
            if (gap > averageGap * 2)
            {
                bus.Publish(new WarningEvent("DataChange too fast."));
            }
        }
    }
}