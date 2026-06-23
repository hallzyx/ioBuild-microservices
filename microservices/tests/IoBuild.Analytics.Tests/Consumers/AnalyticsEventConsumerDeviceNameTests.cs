using FluentAssertions;
using IoBuild.Analytics.Infrastructure.Messaging;
using IoBuild.Analytics.Domain.Model.Projections;
using IoBuild.Shared.Domain.Model.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace IoBuild.Analytics.Tests.Consumers;

/// <summary>
/// E-1 (TDD RED-first): DeviceName mapping in AnalyticsEventConsumer.
///
/// Tests:
///   a) Event with DeviceName → projection.DeviceName populated.
///   b) Event without DeviceName (null) → projection.DeviceName is null.
///   c) Fallback format ("{Type} #{Id}") applied at query layer — NOT in the consumer
///      (consumer stores null; query layer applies fallback). Tested via AnalyticsQueryService.
/// </summary>
public class AnalyticsEventConsumerDeviceNameTests
{
    private static AnalyticsDbContext BuildContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AnalyticsDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AnalyticsDbContext(options);
    }

    private static AnalyticsEventConsumer BuildConsumer(AnalyticsDbContext db) =>
        new(db, NullLogger<AnalyticsEventConsumer>.Instance);

    // ── E-1a: Event with DeviceName → projection.DeviceName populated ─────────

    [Fact]
    public async Task DeviceCreatedEvent_WithDeviceName_PopulatesProjectionDeviceName()
    {
        await using var db = BuildContext(nameof(DeviceCreatedEvent_WithDeviceName_PopulatesProjectionDeviceName));
        var consumer = BuildConsumer(db);

        var evt = new DeviceCreatedEvent
        {
            DeviceId = 1,
            DeviceType = "CO2Sensor",
            Status = "Active",
            UnitId = 10,
            DeviceName = "My CO2 Sensor"
        };

        await consumer.ApplyEventAsync(evt);

        var projection = await db.DeviceProjections.FindAsync(1);
        projection.Should().NotBeNull();
        projection!.DeviceName.Should().Be("My CO2 Sensor");
    }

    // ── E-1b: Event without DeviceName (null) → projection.DeviceName is null ──

    [Fact]
    public async Task DeviceCreatedEvent_WithNullDeviceName_ProjectionDeviceNameIsNull()
    {
        await using var db = BuildContext(nameof(DeviceCreatedEvent_WithNullDeviceName_ProjectionDeviceNameIsNull));
        var consumer = BuildConsumer(db);

        var evt = new DeviceCreatedEvent
        {
            DeviceId = 2,
            DeviceType = "AirConditioner",
            Status = "Online",
            UnitId = 20,
            DeviceName = null   // legacy event — no name carried
        };

        await consumer.ApplyEventAsync(evt);

        var projection = await db.DeviceProjections.FindAsync(2);
        projection.Should().NotBeNull();
        projection!.DeviceName.Should().BeNull(
            "consumer must not invent a DeviceName; fallback is the query layer's job");
    }
}
