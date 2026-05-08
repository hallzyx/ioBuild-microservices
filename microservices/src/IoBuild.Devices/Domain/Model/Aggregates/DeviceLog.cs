namespace IoBuild.Devices.Domain.Model.Aggregates;

public class DeviceLog
{
    public int Id { get; private set; }
    public int DeviceId { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string Value { get; private set; }
    public string Type { get; private set; }
    public string Metadata { get; private set; }

    public DeviceLog(int deviceId, DateTime timestamp, string value, string type, string metadata)
    {
        DeviceId = deviceId;
        Timestamp = timestamp;
        Value = value;
        Type = type;
        Metadata = metadata;
    }

    public void Update(DateTime timestamp, string value, string type, string metadata)
    {
        Timestamp = timestamp;
        Value = value;
        Type = type;
        Metadata = metadata;
    }
}
