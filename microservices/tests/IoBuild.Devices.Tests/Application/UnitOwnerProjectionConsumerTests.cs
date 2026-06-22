using FluentAssertions;
using IoBuild.Devices.Infrastructure.Messaging;
using IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;
using IoBuild.Shared.Domain.Model.Events;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace IoBuild.Devices.Tests.Application;

/// <summary>
/// TDD RED tests for tasks 2.15–2.16 — UnitOwnerProjectionConsumer upsert + idempotency.
///
/// Test seam mirrors UnitDeviceProvisioningConsumer: internal constructor accepts a
/// DevicesDbContext directly so no broker connection is needed.
///
/// Uses EF InMemory (sufficient for the upsert logic — no unique index needed here,
/// the idempotency is achieved via the upsert pattern in the consumer itself).
/// </summary>
public class UnitOwnerProjectionConsumerTests
{
    private static DevicesDbContext BuildContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<DevicesDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;
        return new DevicesDbContext(options);
    }

    private static UnitOwnerProjectionConsumer BuildConsumer(DevicesDbContext db)
    {
        var logger = new Mock<ILogger<UnitOwnerProjectionConsumer>>().Object;
        return new UnitOwnerProjectionConsumer(db, logger);
    }

    // ── 2.15: On UnitOwnerMatchedEvent, row is upserted correctly ─────────────

    /// <summary>
    /// Task 2.15 RED: Processing a UnitOwnerMatchedEvent inserts a row with correct UnitId + OwnerUserId.
    /// </summary>
    [Fact]
    public async Task UnitOwnerProjectionConsumer_OnUnitOwnerMatchedEvent_UpsertsRow()
    {
        await using var db = BuildContext("uop-2.15");
        var consumer = BuildConsumer(db);

        var evt = new UnitOwnerMatchedEvent
        {
            UnitId = 10,
            ProjectId = 1,
            OwnerUserId = 42,
            OwnerEmail = "owner@test.com"
        };

        await consumer.HandleEventAsync(evt);

        var rows = db.UnitOwnerProjections.ToList();
        rows.Should().HaveCount(1, "exactly one projection row must be created");
        rows[0].UnitId.Should().Be(10);
        rows[0].OwnerUserId.Should().Be(42);
        rows[0].UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    // ── 2.16: Idempotent — duplicate event produces exactly one row ────────────

    /// <summary>
    /// Task 2.16 RED: Processing the same event twice results in exactly one row (no duplicates).
    /// </summary>
    [Fact]
    public async Task UnitOwnerProjectionConsumer_Idempotent_NoDuplicateRows()
    {
        await using var db = BuildContext("uop-2.16");
        var consumer = BuildConsumer(db);

        var evt = new UnitOwnerMatchedEvent
        {
            UnitId = 20,
            ProjectId = 2,
            OwnerUserId = 55,
            OwnerEmail = "owner2@test.com"
        };

        await consumer.HandleEventAsync(evt);  // first delivery
        await consumer.HandleEventAsync(evt);  // redelivery — must be idempotent

        var rows = db.UnitOwnerProjections.ToList();
        rows.Should().HaveCount(1, "re-delivery must not create duplicate rows (idempotent)");
        rows[0].UnitId.Should().Be(20);
        rows[0].OwnerUserId.Should().Be(55);
    }
}
