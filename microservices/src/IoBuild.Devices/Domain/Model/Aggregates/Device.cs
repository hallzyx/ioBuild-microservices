namespace IoBuild.Devices.Domain.Model.Aggregates;

public class Device
{
    public int Id { get; private set; }
    public string Name { get; private set; }
    public string Type { get; private set; }
    public string Location { get; private set; }
    public string MacAddress { get; private set; }
    public int ProjectId { get; private set; }
    public string Status { get; private set; }

    public Device(string name, string type, string location, string macAddress, int projectId, string status)
    {
        Name = name;
        Type = type;
        Location = location;
        MacAddress = macAddress;
        ProjectId = projectId;
        Status = status;
    }

    public void Update(string name, string type, string location, string macAddress, int projectId, string status)
    {
        Name = name;
        Type = type;
        Location = location;
        MacAddress = macAddress;
        ProjectId = projectId;
        Status = status;
    }
}
