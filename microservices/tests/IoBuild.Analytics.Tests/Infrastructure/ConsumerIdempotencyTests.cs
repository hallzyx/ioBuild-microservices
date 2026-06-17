using FluentAssertions;
using IoBuild.Analytics.Domain.Model.Projections;
using IoBuild.Analytics.Infrastructure.Messaging;
using IoBuild.Shared.Domain.Model.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace IoBuild.Analytics.Tests.Infrastructure;

/// <summary>
/// REQ-RM-03, RM-S02 — Consumer idempotency: same DeviceCreatedEvent delivered twice
/// must result in exactly one DeviceProjection row with correct fields.
/// </summary>
public class ConsumerIdempotencyTests
{
    private static AnalyticsDbContext BuildContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AnalyticsDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AnalyticsDbContext(options);
    }

    [Fact]
    public async Task DeviceCreated_delivered_twice_results_in_one_projection_row()
    {
        // Arrange
        await using var db = BuildContext(nameof(DeviceCreated_delivered_twice_results_in_one_projection_row));

        var evt = new DeviceCreatedEvent
        {
            DeviceId = 42,
            OwnerUserId = 0,
            ProjectId = 7,
            UnitId = null,
            DeviceType = "Temperature",
            Status = "Online"
        };

        var consumer = new AnalyticsEventConsumer(db, NullLogger<AnalyticsEventConsumer>.Instance);

        // Act — deliver the same event twice
        await consumer.ApplyEventAsync(evt);
        await consumer.ApplyEventAsync(evt);

        // Assert — exactly one row with correct fields
        var rows = await db.DeviceProjections.ToListAsync();
        rows.Should().HaveCount(1);
        rows[0].DeviceId.Should().Be(42);
        rows[0].ProjectId.Should().Be(7);
        rows[0].DeviceType.Should().Be("Temperature");
        rows[0].Status.Should().Be("Online");
    }

    [Fact]
    public async Task DeviceCreated_then_stale_event_leaves_row_unchanged()
    {
        // Arrange — REQ-RM-03, RM-S02b: stale event (older OccurredOn) must be discarded
        await using var db = BuildContext(nameof(DeviceCreated_then_stale_event_leaves_row_unchanged));

        var t2 = DateTime.UtcNow;
        var t1 = t2.AddSeconds(-30); // older

        // Seed a row with a newer timestamp
        db.DeviceProjections.Add(new DeviceProjection
        {
            DeviceId = 10,
            OwnerUserId = 0,
            ProjectId = 1,
            DeviceType = "Energy",
            Status = "Online",
            LastEventAt = t2
        });
        await db.SaveChangesAsync();

        var staleEvent = new DeviceCreatedEvent
        {
            DeviceId = 10,
            OwnerUserId = 0,
            ProjectId = 1,
            DeviceType = "Energy",
            Status = "Offline" // would change status — but should be discarded
        } with { OccurredOn = t1 };

        var consumer = new AnalyticsEventConsumer(db, NullLogger<AnalyticsEventConsumer>.Instance);

        // Act
        await consumer.ApplyEventAsync(staleEvent);

        // Assert — status must remain "Online" (stale event discarded)
        var row = await db.DeviceProjections.FindAsync(10);
        row!.Status.Should().Be("Online");
        row.LastEventAt.Should().Be(t2);
    }
}
