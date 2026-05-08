namespace IoBuild.Analytics.Domain.Model.Entities;

public class DeviceHealthStatus
{
    public int DeviceId { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime LastOnline { get; set; }
}
