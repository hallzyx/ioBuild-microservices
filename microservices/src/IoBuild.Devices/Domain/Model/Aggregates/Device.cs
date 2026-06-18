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

    // Floor placement fields — added in PR 6 (§4.4).
    // FloorNumber: set by FloorProvisioningConsumer when a device is auto-provisioned per floor.
    // UnitId: reserved for future per-unit device placement; null until linked to a specific unit.
    public int? FloorNumber { get; private set; }
    public int? UnitId { get; private set; }

    /// <summary>
    /// Standard constructor for manually-created devices (no floor placement).
    /// </summary>
    public Device(string name, string type, string location, string macAddress, int projectId, string status)
    {
        Name = name;
        Type = type;
        Location = location;
        MacAddress = macAddress;
        ProjectId = projectId;
        Status = status;
        FloorNumber = null;
        UnitId = null;
    }

    /// <summary>
    /// Extended constructor for floor-provisioned devices. Accepts nullable FloorNumber and UnitId (§4.4).
    /// </summary>
    public Device(string name, string type, string location, string macAddress, int projectId, string status,
        int? floorNumber, int? unitId = null)
    {
        Name = name;
        Type = type;
        Location = location;
        MacAddress = macAddress;
        ProjectId = projectId;
        Status = status;
        FloorNumber = floorNumber;
        UnitId = unitId;
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
