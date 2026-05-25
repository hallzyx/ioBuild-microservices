using IoBuild.Devices.Domain.Model.Aggregates;
using IoBuild.Devices.Domain.Model.Queries;
using IoBuild.Devices.Domain.Services;
using FluentAssertions;

namespace IoBuild.Devices.Tests.UnitTests;

public class TelemetryQueryServiceTests
{
    [Fact]
    public void GetDeviceEnergyQuery_WithDifferentDates_ShouldReturnCorrectValues()
    {
        var from = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var to = new DateTime(2025, 1, 2, 0, 0, 0, DateTimeKind.Utc);
        var query = new GetDeviceEnergyQuery(5, from, to);

        query.DeviceId.Should().Be(5);
        query.From.Should().Be(from);
        query.To.Should().Be(to);
    }

    [Fact]
    public void GetDeviceStatusQuery_WithDifferentId_ShouldStoreCorrectly()
    {
        var query = new GetDeviceStatusQuery(99);
        query.DeviceId.Should().Be(99);
    }
}
