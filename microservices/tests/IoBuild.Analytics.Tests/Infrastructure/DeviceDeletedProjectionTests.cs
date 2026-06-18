using FluentAssertions;
using IoBuild.Analytics.Domain.Model.Projections;
using IoBuild.Analytics.Infrastructure.Messaging;
using IoBuild.Shared.Domain.Model.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace IoBuild.Analytics.Tests.Infrastructure;

/// <summary>
/// REQ-RM-04, AQ-S05 — DeviceDeleted removes the projection row;
/// deleting an absent row must be a no-op (idempotent).
/// </summary>
public class DeviceDeletedProjectionTests
{
    private static AnalyticsDbContext BuildContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AnalyticsDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AnalyticsDbContext(options);
    }

    [Fact]
    public async Task DeviceDeleted_removes_existing_projection_row()
    {
        // Arrange
        await using var db = BuildContext(nameof(DeviceDeleted_removes_existing_projection_row));

        db.DeviceProjections.Add(new DeviceProjection
        {
            DeviceId = 99,
            OwnerUserId = 0,
            DeviceType = "Water",
            Status = "Online",
            LastEventAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();

        var consumer = new AnalyticsEventConsumer(db, NullLogger<AnalyticsEventConsumer>.Instance);
        var deletedEvent = new DeviceDeletedEvent { DeviceId = 99, OwnerUserId = 0 };

        // Act
        await consumer.ApplyEventAsync(deletedEvent);

        // Assert
        var rows = await db.DeviceProjections.ToListAsync();
        rows.Should().BeEmpty();
    }

    [Fact]
    public async Task DeviceDeleted_absent_row_is_noop()
    {
        // Arrange
        await using var db = BuildContext(nameof(DeviceDeleted_absent_row_is_noop));

        var consumer = new AnalyticsEventConsumer(db, NullLogger<AnalyticsEventConsumer>.Instance);
        var deletedEvent = new DeviceDeletedEvent { DeviceId = 404, OwnerUserId = 0 };

        // Act — must not throw
        var act = () => consumer.ApplyEventAsync(deletedEvent);

        // Assert
        await act.Should().NotThrowAsync();
        (await db.DeviceProjections.ToListAsync()).Should().BeEmpty();
    }
}
