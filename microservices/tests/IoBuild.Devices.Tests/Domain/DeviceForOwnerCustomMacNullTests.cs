using FluentAssertions;
using IoBuild.Devices.Domain.Model.Aggregates;

namespace IoBuild.Devices.Tests.Domain;

/// <summary>
/// MAC-null fix (TDD RED-first): Device.ForOwnerCustom must store MacAddress = null.
///
/// Owner-custom devices are virtual — they have no real hardware MAC.
/// Storing '' (empty string) caused a UNIQUE constraint collision on IX_devices_mac_address
/// the moment a second owner-custom device was created anywhere in the system.
/// The fix: MacAddress property is nullable (string?) and ForOwnerCustom sets it to null.
/// MySQL UNIQUE indexes permit multiple NULLs; real devices keep their unique MACs.
/// </summary>
public class DeviceForOwnerCustomMacNullTests
{
    // ── MAC-1: ForOwnerCustom stores null MacAddress (core regression guard) ───

    [Fact]
    public void ForOwnerCustom_MacAddressIsNull()
    {
        var device = Device.ForOwnerCustom(
            name: "My CO2 Sensor",
            type: "CO2Monitor",
            location: "Unit 01",
            projectId: 1,
            status: "Active",
            unitId: 42);

        device.MacAddress.Should().BeNull(
            "owner-custom devices have no hardware MAC; storing '' caused IX_devices_mac_address collisions");
    }

    // ── MAC-2: Regular Device constructor still accepts a real MAC ─────────────

    [Fact]
    public void StandardDevice_StoresExplicitMac()
    {
        var device = new Device("Thermostat", "Thermostat", "Floor 1",
            "AA:BB:CC:DD:EE:FF", projectId: 1, status: "Active");

        device.MacAddress.Should().Be("AA:BB:CC:DD:EE:FF");
    }

    // ── MAC-3: ForUnitPackage still stores the provided (deterministic) MAC ────

    [Fact]
    public void ForUnitPackage_StoresProvidedMac()
    {
        var device = Device.ForUnitPackage(
            "AC Unit 01", "AirConditioner", "Floor 2",
            "U1:00:00:00:01:AC", projectId: 1, status: "Active",
            floorNumber: 2, unitId: 7);

        device.MacAddress.Should().Be("U1:00:00:00:01:AC");
    }
}
