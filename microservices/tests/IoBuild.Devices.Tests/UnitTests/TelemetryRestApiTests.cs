using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Interfaces.REST.Resources;
using IoBuild.Devices.Interfaces.REST.Transform;
using FluentAssertions;

namespace IoBuild.Devices.Tests.UnitTests;

public class TelemetryRestApiTests
{
    [Fact]
    public void EnergyDataResource_ShouldStoreValues()
    {
        var now = DateTime.UtcNow;
        var resource = new EnergyDataResource(now, 1.5, 22.5, 220.0);

        resource.Timestamp.Should().Be(now);
        resource.EnergyKwh.Should().Be(1.5);
        resource.TemperatureC.Should().Be(22.5);
        resource.VoltageV.Should().Be(220.0);
    }

    [Fact]
    public void DeviceStatusResource_ShouldStoreValues()
    {
        var now = DateTime.UtcNow;
        var resource = new DeviceStatusResource(1, "online", now, 22.5, 220.0);

        resource.DeviceId.Should().Be(1);
        resource.Status.Should().Be("online");
        resource.LastSeen.Should().Be(now);
        resource.TemperatureC.Should().Be(22.5);
        resource.VoltageV.Should().Be(220.0);
    }

    [Fact]
    public void TelemetryResourceAssembler_ToEnergyResource_ShouldMapCorrectly()
    {
        var now = DateTime.UtcNow;
        var point = new EnergyDataPoint(now, 2.5, 30.0, 225.0);

        var resource = TelemetryResourceAssembler.ToEnergyResource(point);

        resource.Timestamp.Should().Be(now);
        resource.EnergyKwh.Should().Be(2.5);
        resource.TemperatureC.Should().Be(30.0);
        resource.VoltageV.Should().Be(225.0);
    }

    [Fact]
    public void TelemetryResourceAssembler_ToStatusResource_ShouldMapCorrectly()
    {
        var now = DateTime.UtcNow;
        var report = new DeviceStatusReport(3, "idle", now, 28.0, 218.0);

        var resource = TelemetryResourceAssembler.ToStatusResource(report);

        resource.DeviceId.Should().Be(3);
        resource.Status.Should().Be("idle");
        resource.LastSeen.Should().Be(now);
        resource.TemperatureC.Should().Be(28.0);
        resource.VoltageV.Should().Be(218.0);
    }

    [Fact]
    public void TelemetryResourceAssembler_ToStatusResource_WithNullReport_ShouldThrow()
    {
        Action act = () => TelemetryResourceAssembler.ToStatusResource(null!);
        act.Should().Throw<ArgumentNullException>();
    }
}
