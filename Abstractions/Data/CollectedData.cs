namespace Abstractions.Data;

public record CollectedData
{
    public string DeviceId { get; }
    public bool Running { get; }
    public float Speed { get; }
    public float Load { get; }
    public float RunningTime { get; }
    
    public float Temperature { get; }
    public float Humidity { get; }
    public float SmokeConcentration { get; }
}