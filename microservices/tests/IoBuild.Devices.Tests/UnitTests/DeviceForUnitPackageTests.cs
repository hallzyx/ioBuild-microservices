using FluentAssertions;
using IoBuild.Devices.Domain.Model.Aggregates;

namespace IoBuild.Devices.Tests.UnitTests;

/// <summary>
/// T-12 (RED): Tests for Device.ForUnitPackage factory method.
/// Verifies: UnitId set, Source="UnitPackage", FloorNumber set, floor ctor unchanged.
/// </summary>
public class DeviceForUnitPackageTests
{
    // ── T12-01: ForUnitPackage sets Source = "UnitPackage" ──────────────────

    [Fact]
    public void ForUnitPackage_Sets_Source_UnitPackage()
    {
        var device = Device.ForUnitPackage(
            name: "Air Conditioner - Unit 01",
            type: "AirConditioner",
            location: "Floor 1 - Room 01",
            mac: "U1:AA:BB:CC:DD:01",
            projectId: 10,
            status: "Active",
            floorNumber: 1,
            unitId: 42);

        device.Source.Should().Be("UnitPackage",
            "ForUnitPackage factory must set Source='UnitPackage' (ADR-3/D3)");
    }

    // ── T12-02: ForUnitPackage sets UnitId correctly ─────────────────────────

    [Fact]
    public void ForUnitPackage_Sets_UnitId()
    {
        var device = Device.ForUnitPackage(
            name: "Smart Light - Unit 01",
            type: "SmartLight",
            location: "Floor 2 - Room 02",
            mac: "U1:AA:BB:CC:DD:02",
            projectId: 10,
            status: "Active",
            floorNumber: 2,
            unitId: 99);

        device.UnitId.Should().Be(99, "UnitId must be set to the real Phase-1 unit id");
    }

    // ── T12-03: ForUnitPackage sets FloorNumber correctly ───────────────────

    [Fact]
    public void ForUnitPackage_Sets_FloorNumber()
    {
        var device = Device.ForUnitPackage(
            name: "Air Conditioner - Unit 03",
            type: "AirConditioner",
            location: "Floor 3 - Room 03",
            mac: "U1:AA:BB:CC:DD:03",
            projectId: 10,
            status: "Active",
            floorNumber: 3,
            unitId: 7);

        device.FloorNumber.Should().Be(3, "FloorNumber must be set from the triggering event's Floor field");
    }

    // ── T12-04: ForUnitPackage sets core fields (Name, Type, ProjectId, Status, MacAddress) ──

    [Fact]
    public void ForUnitPackage_Sets_CoreFields()
    {
        var device = Device.ForUnitPackage(
            name: "Smart Light - Unit 04",
            type: "SmartLight",
            location: "Floor 1 - Room 04",
            mac: "U1:00:11:22:33:44",
            projectId: 20,
            status: "Active",
            floorNumber: 1,
            unitId: 5);

        device.Name.Should().Be("Smart Light - Unit 04");
        device.Type.Should().Be("SmartLight");
        device.ProjectId.Should().Be(20);
        device.Status.Should().Be("Active");
        device.MacAddress.Should().Be("U1:00:11:22:33:44");
        device.Location.Should().Be("Floor 1 - Room 04");
    }

    // ── T12-05: floor-provisioned constructor is UNCHANGED (Source still "FloorProvisioned") ──

    [Fact]
    public void FloorProvisionedConstructor_Source_StillFloorProvisioned_Unchanged()
    {
        var floorDevice = new Device(
            name: "Smart Meter - Floor 1",
            type: "SmartMeter",
            location: "Floor 1",
            macAddress: "F1:AA:BB:CC:DD:EE",
            projectId: 1,
            status: "Active",
            floorNumber: 1);

        floorDevice.Source.Should().Be("FloorProvisioned",
            "The floor-provisioned constructor MUST remain unchanged — ForUnitPackage must not alter it");
        floorDevice.UnitId.Should().BeNull("floor devices have no UnitId");
    }

    // ── T12-06: MAC prefix "U1:" distinguishes unit devices from floor devices ("F1:") ──

    [Fact]
    public void ForUnitPackage_MacPrefix_Differs_FromFloorMac()
    {
        var unitDevice = Device.ForUnitPackage(
            name: "Air Conditioner - Unit 01",
            type: "AirConditioner",
            location: "Floor 1 - Room 01",
            mac: "U1:AA:BB:CC:DD:05",
            projectId: 1,
            status: "Active",
            floorNumber: 1,
            unitId: 1);

        var floorDevice = new Device(
            name: "Smart Meter - Floor 1",
            type: "SmartMeter",
            location: "Floor 1",
            macAddress: "F1:AA:BB:CC:DD:EE",
            projectId: 1,
            status: "Active",
            floorNumber: 1);

        unitDevice.MacAddress.Should().StartWith("U1:",
            "unit-package MACs are prefixed 'U1:' to distinguish from floor-provisioned 'F1:' MACs");
        floorDevice.MacAddress.Should().StartWith("F1:",
            "floor MACs still start with 'F1:'");
        unitDevice.MacAddress.Should().NotBe(floorDevice.MacAddress,
            "unit and floor MACs on the same project/floor must not collide");
    }
}
