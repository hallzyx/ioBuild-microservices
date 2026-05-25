using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Domain.Model.Queries;
using IoBuild.Devices.Domain.Services;
using FluentAssertions;

namespace IoBuild.Devices.Tests.UnitTests;

public class TelemetryDomainTests
{
    [Fact]
    public void EnergyDataPoint_ShouldStoreValues()
    {
        var now = DateTime.UtcNow;
        var point = new EnergyDataPoint(now, 1.5, 22.5, 220.0);

        point.Timestamp.Should().Be(now);
        point.EnergyKwh.Should().Be(1.5);
        point.TemperatureC.Should().Be(22.5);
        point.VoltageV.Should().Be(220.0);
    }

    [Fact]
    public void EnergyDataPoint_ShouldBeRecordWithValueEquality()
    {
        var now = DateTime.UtcNow;
        var p1 = new EnergyDataPoint(now, 1.5, 22.5, 220.0);
        var p2 = new EnergyDataPoint(now, 1.5, 22.5, 220.0);

        p1.Should().Be(p2);
        (p1 == p2).Should().BeTrue();
    }

    [Fact]
    public void DeviceStatusReport_ShouldStoreValues()
    {
        var now = DateTime.UtcNow;
        var report = new DeviceStatusReport(1, "online", now, 22.5, 220.0);

        report.DeviceId.Should().Be(1);
        report.Status.Should().Be("online");
        report.LastSeen.Should().Be(now);
        report.TemperatureC.Should().Be(22.5);
        report.VoltageV.Should().Be(220.0);
    }

    [Fact]
    public void DeviceStatusReport_ShouldBeRecordWithValueEquality()
    {
        var now = DateTime.UtcNow;
        var r1 = new DeviceStatusReport(1, "online", now, 22.5, 220.0);
        var r2 = new DeviceStatusReport(1, "online", now, 22.5, 220.0);

        r1.Should().Be(r2);
        (r1 == r2).Should().BeTrue();
    }

    [Fact]
    public void GetDeviceEnergyQuery_ShouldStoreValues()
    {
        var from = DateTime.UtcNow.AddHours(-24);
        var to = DateTime.UtcNow;
        var query = new GetDeviceEnergyQuery(1, from, to);

        query.DeviceId.Should().Be(1);
        query.From.Should().Be(from);
        query.To.Should().Be(to);
    }

    [Fact]
    public void GetDeviceStatusQuery_ShouldStoreValues()
    {
        var query = new GetDeviceStatusQuery(42);

        query.DeviceId.Should().Be(42);
    }

    [Fact]
    public void ITelemetryQueryService_ShouldDefineHandleMethods()
    {
        var serviceType = typeof(ITelemetryQueryService);

        var energyMethod = serviceType.GetMethod("Handle",
            new[] { typeof(GetDeviceEnergyQuery) });
        energyMethod.Should().NotBeNull();
        energyMethod!.ReturnType.Should().Be(typeof(Task<IEnumerable<EnergyDataPoint>>));
        energyMethod.GetParameters()[0].ParameterType.Should().Be(typeof(GetDeviceEnergyQuery));

        var statusMethod = serviceType.GetMethod("Handle",
            new[] { typeof(GetDeviceStatusQuery) });
        statusMethod.Should().NotBeNull();
        statusMethod!.ReturnType.Should().Be(typeof(Task<DeviceStatusReport?>));
        statusMethod.GetParameters()[0].ParameterType.Should().Be(typeof(GetDeviceStatusQuery));
    }
}
