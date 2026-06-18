using FluentAssertions;
using IoBuild.Analytics.Domain.Model.Projections;
using IoBuild.Analytics.Infrastructure.Messaging;
using IoBuild.Shared.Domain.Model.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace IoBuild.Analytics.Tests.Infrastructure;

/// <summary>
/// PR 7 — Analytics Projection Updates (tasks 7.1–7.5).
/// Tests use the internal-constructor consumer seam (direct AnalyticsDbContext).
/// </summary>
public class AnalyticsProjectionUpdateTests
{
    private static AnalyticsDbContext BuildContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AnalyticsDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AnalyticsDbContext(options);
    }

    // ── 7.1a — UnitCreatedEvent maps Floor, RoomNumber, OwnerEmail ──

    [Fact]
    public async Task UnitCreated_with_floor_and_owner_email_maps_projection_fields()
    {
        await using var db = BuildContext(nameof(UnitCreated_with_floor_and_owner_email_maps_projection_fields));
        var consumer = new AnalyticsEventConsumer(db, NullLogger<AnalyticsEventConsumer>.Instance);

        var evt = new UnitCreatedEvent
        {
            UnitId        = 1,
            ProjectId     = 10,
            BuilderUserId = 5,
            Status        = "Available",
            Floor         = 3,
            RoomNumber    = "301",
            OwnerEmail    = "x@y.com"
        };

        await consumer.ApplyEventAsync(evt);

        var row = await db.UnitProjections.FindAsync(1);
        row.Should().NotBeNull();
        row!.Floor.Should().Be(3);
        row.RoomNumber.Should().Be("301");
        row.OwnerEmail.Should().Be("x@y.com");
    }

    // ── 7.1b — UnitOwnerMatchedEvent on existing projection sets OwnerUserId ──

    [Fact]
    public async Task UnitOwnerMatched_on_existing_projection_sets_owner_user_id()
    {
        await using var db = BuildContext(nameof(UnitOwnerMatched_on_existing_projection_sets_owner_user_id));

        // Seed an existing projection row
        db.UnitProjections.Add(new UnitProjection
        {
            UnitId        = 2,
            ProjectId     = 10,
            BuilderUserId = 5,
            Status        = "Available",
            LastEventAt   = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
        await db.SaveChangesAsync();

        var consumer = new AnalyticsEventConsumer(db, NullLogger<AnalyticsEventConsumer>.Instance);

        var evt = new UnitOwnerMatchedEvent
        {
            UnitId       = 2,
            ProjectId    = 10,
            OwnerUserId  = 42,
            OwnerEmail   = "alice@test.com"
        };

        await consumer.ApplyEventAsync(evt);

        var row = await db.UnitProjections.FindAsync(2);
        row!.OwnerUserId.Should().Be(42);
    }

    // ── 7.1c — Out-of-order: UnitOwnerMatchedEvent arrives BEFORE UnitCreatedEvent ──

    [Fact]
    public async Task UnitOwnerMatched_before_UnitCreated_creates_row_then_enriched_by_unit_created()
    {
        await using var db = BuildContext(nameof(UnitOwnerMatched_before_UnitCreated_creates_row_then_enriched_by_unit_created));
        var consumer = new AnalyticsEventConsumer(db, NullLogger<AnalyticsEventConsumer>.Instance);

        var t1 = new DateTime(2026, 6, 1, 10, 0, 0, DateTimeKind.Utc);
        var t2 = new DateTime(2026, 6, 1, 10, 5, 0, DateTimeKind.Utc);

        // Owner-matched arrives first (t1)
        var ownerEvt = new UnitOwnerMatchedEvent
        {
            UnitId      = 3,
            ProjectId   = 10,
            OwnerUserId = 42,
            OwnerEmail  = "bob@test.com"
        } with { OccurredOn = t1 };

        await consumer.ApplyEventAsync(ownerEvt);

        // Verify placeholder row exists with OwnerUserId set
        var rowAfterOwner = await db.UnitProjections.FindAsync(3);
        rowAfterOwner.Should().NotBeNull();
        rowAfterOwner!.OwnerUserId.Should().Be(42);

        // UnitCreated arrives later (t2 > t1) — should enrich but NOT clobber OwnerUserId
        var unitEvt = new UnitCreatedEvent
        {
            UnitId        = 3,
            ProjectId     = 10,
            BuilderUserId = 5,
            Status        = "Available",
            Floor         = 1,
            RoomNumber    = "101",
            OwnerEmail    = "bob@test.com"
        } with { OccurredOn = t2 };

        await consumer.ApplyEventAsync(unitEvt);

        var row = await db.UnitProjections.FindAsync(3);
        row!.Floor.Should().Be(1);
        row.RoomNumber.Should().Be("101");
        row.OwnerEmail.Should().Be("bob@test.com");
        row.BuilderUserId.Should().Be(5);
        // OwnerUserId from the matched event is preserved (UnitCreatedEvent.OwnerUserId is null here)
        row.OwnerUserId.Should().Be(42);
    }

    // ── 7.1d — DeviceCreatedEvent maps FloorNumber ──

    [Fact]
    public async Task DeviceCreated_with_floor_number_maps_projection_field()
    {
        await using var db = BuildContext(nameof(DeviceCreated_with_floor_number_maps_projection_field));
        var consumer = new AnalyticsEventConsumer(db, NullLogger<AnalyticsEventConsumer>.Instance);

        var evt = new DeviceCreatedEvent
        {
            DeviceId    = 50,
            OwnerUserId = 0,
            ProjectId   = 10,
            DeviceType  = "SmartMeter",
            Status      = "Online",
            FloorNumber = 2
        };

        await consumer.ApplyEventAsync(evt);

        var row = await db.DeviceProjections.FindAsync(50);
        row.Should().NotBeNull();
        row!.FloorNumber.Should().Be(2);
    }
}
