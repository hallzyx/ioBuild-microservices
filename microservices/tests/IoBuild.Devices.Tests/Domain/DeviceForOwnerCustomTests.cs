using FluentAssertions;
using IoBuild.Devices.Domain.Model.Aggregates;

namespace IoBuild.Devices.Tests.Domain;

/// <summary>
/// D-1 (TDD RED-first): Device.ForOwnerCustom factory method tests.
/// Pure domain — no DB required.
/// </summary>
public class DeviceForOwnerCustomTests
{
    // ── D-1a: UnitId is set on returned device ─────────────────────────────────

    [Fact]
    public void ForOwnerCustom_SetsUnitId()
    {
        var device = Device.ForOwnerCustom(
            name: "My CO2 Sensor",
            type: "CO2Monitor",
            location: "Unit 01",
            mac: "OC:AA:BB:CC:DD:01",
            projectId: 1,
            status: "Active",
            unitId: 42);

        device.UnitId.Should().Be(42);
    }

    // ── D-1b: Source is "OwnerCustom" ─────────────────────────────────────────

    [Fact]
    public void ForOwnerCustom_SetsSourceToOwnerCustom()
    {
        var device = Device.ForOwnerCustom("My Sensor", "CO2Monitor", "Unit 01",
            "OC:AA:BB:CC:DD:02", 1, "Active", unitId: 5);

        device.Source.Should().Be("OwnerCustom");
    }

    // ── D-1c: Name is set to the user-given name ──────────────────────────────

    [Fact]
    public void ForOwnerCustom_SetsName()
    {
        var device = Device.ForOwnerCustom("My CO2 Sensor", "CO2Monitor", "Unit 01",
            "OC:AA:BB:CC:DD:03", 1, "Active", unitId: 5);

        device.Name.Should().Be("My CO2 Sensor");
    }

    // ── D-1d: Type is set ─────────────────────────────────────────────────────

    [Fact]
    public void ForOwnerCustom_SetsType()
    {
        var device = Device.ForOwnerCustom("My Sensor", "CO2Monitor", "Unit 01",
            "OC:AA:BB:CC:DD:04", 1, "Active", unitId: 5);

        device.Type.Should().Be("CO2Monitor");
    }
}
