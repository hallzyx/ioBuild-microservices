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
    /// Provenance discriminator (S5.1). Values:
    ///   "FloorProvisioned" — created by FloorProvisioningConsumer via event-driven provisioning.
    ///   "OwnerCustom"      — reserved for future owner-created devices (not set in this change).
    ///   null               — legacy devices created before this field existed, or manual POST devices.
    /// Nullable column: existing rows and EnsureCreated test paths remain valid without backfill (S5.2).
    /// </summary>
    public string? Source { get; private set; }

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
    /// Sets Source = "FloorProvisioned" to mark provenance (S5.1).
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
        Source = "FloorProvisioned";
    }

    /// <summary>
    /// Factory method for unit-package provisioned devices (ADR-3/D3, T-12).
    /// Sets UnitId + Source="UnitPackage" + FloorNumber; leaves the floor-provisioned
    /// constructor UNCHANGED so existing provisioning is unaffected.
    /// </summary>
    /// <param name="name">Display name (e.g. "Air Conditioner - Unit 01").</param>
    /// <param name="type">Device type code from DeviceTypeCatalog.KnownTypes.</param>
    /// <param name="location">Human-readable location string.</param>
    /// <param name="mac">Deterministic MAC — must use "U1:" prefix to avoid floor-MAC collisions.</param>
    /// <param name="projectId">Project the unit belongs to.</param>
    /// <param name="status">Initial device status.</param>
    /// <param name="floorNumber">Floor carried from UnitDevicesDefinedEvent.Floor.</param>
    /// <param name="unitId">Real Phase-1 unit DB id — the provisioning anchor.</param>
    public static Device ForUnitPackage(
        string name, string type, string location, string mac,
        int projectId, string status, int floorNumber, int unitId)
    {
        var d = new Device(name, type, location, mac, projectId, status);
        d.FloorNumber = floorNumber;
        d.UnitId = unitId;
        d.Source = "UnitPackage";
        return d;
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
