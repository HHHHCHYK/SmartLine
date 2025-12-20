namespace Abstractions.Data;

public class CollectedData
{
    public required string DeviceId { get; set; }
    public bool Running { get; set; }
    public float Speed { get; set; }
    public float Load { get; set; }
    public float RunningTime { get; set; }
    
    public float Temperature { get; set; }
    public float Humidity { get; set; }
    public float SmokeConcentration { get; set; }
    public float Pressure { get; set; }
}