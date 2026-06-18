using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Domain.Model.Entities;
using IoBuild.Projects.Infrastructure.Messaging;
using IoBuild.Projects.Infrastructure.Persistence;
using IoBuild.Projects.Tests.Infrastructure;
using IoBuild.Shared.Domain.Model.Events;
using Microsoft.Extensions.Logging.Abstractions;

namespace IoBuild.Projects.Tests.Application;

/// <summary>
/// Unit tests for OwnerLinkingConsumer (PR 5, §3.1–3.2).
/// Uses the internal test constructor seam — no live RabbitMQ broker needed.
/// Tests call ApplyEventAsync directly with a pre-wired AppDbContext (mirrors
/// the AnalyticsEventConsumer test pattern, §9.3).
/// </summary>
public class OwnerLinkingConsumerTests
{
    // ------------------------------------------------------------------ //
    //  OL-S01 — Unit-first: owner registers after unit is email-assigned
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task OL_S01_UnitFirst_SetsOwnerId_AndEmitsOutboxEvent()
    {
        // Arrange
        var dbName = $"{nameof(OL_S01_UnitFirst_SetsOwnerId_AndEmitsOutboxEvent)}_{Guid.NewGuid()}";
        await using var ctx = ProjectsDbFixture.NewContext(dbName);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx);
        var unit = await ProjectsDbFixture.SeedStructuredUnitAsync(ctx, project.Id, floor: 1, roomNumber: "01",
            ownerEmail: "alice@test.com");

        Assert.Null(unit.OwnerId);

        var consumer = new OwnerLinkingConsumer(ctx, NullLogger<OwnerLinkingConsumer>.Instance);
        var evt = new UserRegisteredEvent { UserId = 42, Email = "alice@test.com", Role = "Owner" };

        // Act
        await consumer.ApplyEventAsync(evt);

        // Assert — reload from same context
        await using var readCtx = ProjectsDbFixture.NewContext(dbName);
        var updatedUnit = await readCtx.Units.FindAsync(unit.Id);
        Assert.NotNull(updatedUnit);
        Assert.Equal(42, updatedUnit.OwnerId);

        var outboxRow = readCtx.OutboxMessages.SingleOrDefault(m => m.EventType == nameof(UnitOwnerMatchedEvent));
        Assert.NotNull(outboxRow);
        Assert.Contains("alice@test.com", outboxRow.Payload);
    }

    // ------------------------------------------------------------------ //
    //  OL-S02 — Registration-first: upserts registered_owner row
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task OL_S02_RegistrationFirst_UpsertsMirrorRow_WhenNoUnitMatches()
    {
        // Arrange — no unit with that email exists
        var dbName = $"{nameof(OL_S02_RegistrationFirst_UpsertsMirrorRow_WhenNoUnitMatches)}_{Guid.NewGuid()}";
        await using var ctx = ProjectsDbFixture.NewContext(dbName);

        var consumer = new OwnerLinkingConsumer(ctx, NullLogger<OwnerLinkingConsumer>.Instance);
        var evt = new UserRegisteredEvent { UserId = 42, Email = "bob@test.com", Role = "Owner" };

        // Act
        await consumer.ApplyEventAsync(evt);

        // Assert — registered_owner row upserted
        await using var readCtx = ProjectsDbFixture.NewContext(dbName);
        var mirrorRow = await readCtx.RegisteredOwners.FindAsync("bob@test.com");
        Assert.NotNull(mirrorRow);
        Assert.Equal(42, mirrorRow.UserId);

        // No outbox row — no unit matched
        Assert.Empty(readCtx.OutboxMessages);
    }

    // ------------------------------------------------------------------ //
    //  OL-S03 — Case-insensitive: Carol@Test.COM matches carol@test.com
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task OL_S03_CaseInsensitive_MatchesUnit_WhenEmailCasingDiffers()
    {
        // Arrange — unit has mixed-case email stored lower-cased (ADR-D)
        var dbName = $"{nameof(OL_S03_CaseInsensitive_MatchesUnit_WhenEmailCasingDiffers)}_{Guid.NewGuid()}";
        await using var ctx = ProjectsDbFixture.NewContext(dbName);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx);
        // The Unit ctor lower-cases ownerEmail — "Carol@Test.COM" → "carol@test.com"
        var unit = await ProjectsDbFixture.SeedStructuredUnitAsync(ctx, project.Id, floor: 1, roomNumber: "02",
            ownerEmail: "Carol@Test.COM");

        var consumer = new OwnerLinkingConsumer(ctx, NullLogger<OwnerLinkingConsumer>.Instance);
        // Event email is already lower-cased (IAM invariant)
        var evt = new UserRegisteredEvent { UserId = 77, Email = "carol@test.com", Role = "Owner" };

        // Act
        await consumer.ApplyEventAsync(evt);

        // Assert
        await using var readCtx = ProjectsDbFixture.NewContext(dbName);
        var updatedUnit = await readCtx.Units.FindAsync(unit.Id);
        Assert.NotNull(updatedUnit);
        Assert.Equal(77, updatedUnit.OwnerId);

        var outboxRow = readCtx.OutboxMessages.SingleOrDefault(m => m.EventType == nameof(UnitOwnerMatchedEvent));
        Assert.NotNull(outboxRow);
    }

    // ------------------------------------------------------------------ //
    //  OL-S04 — Non-owner role: no mutation, acked
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task OL_S04_NonOwnerRole_NoMutation_NoOutboxRow()
    {
        // Arrange — a unit with matching email exists, but role is Builder
        var dbName = $"{nameof(OL_S04_NonOwnerRole_NoMutation_NoOutboxRow)}_{Guid.NewGuid()}";
        await using var ctx = ProjectsDbFixture.NewContext(dbName);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx);
        var unit = await ProjectsDbFixture.SeedStructuredUnitAsync(ctx, project.Id, floor: 1, roomNumber: "03",
            ownerEmail: "builder@test.com");

        var consumer = new OwnerLinkingConsumer(ctx, NullLogger<OwnerLinkingConsumer>.Instance);
        var evt = new UserRegisteredEvent { UserId = 99, Email = "builder@test.com", Role = "Builder" };

        // Act
        await consumer.ApplyEventAsync(evt);

        // Assert — unit untouched, no outbox row, no registered_owner mirror
        await using var readCtx = ProjectsDbFixture.NewContext(dbName);
        var unchangedUnit = await readCtx.Units.FindAsync(unit.Id);
        Assert.NotNull(unchangedUnit);
        Assert.Null(unchangedUnit.OwnerId);
        Assert.Empty(readCtx.OutboxMessages);
        Assert.Null(await readCtx.RegisteredOwners.FindAsync(evt.Email));
    }

    // ------------------------------------------------------------------ //
    //  OL-S05 — Idempotent re-delivery: no overwrite, no duplicate event
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task OL_S05_Redelivery_OwnerId_AlreadySet_NoOverwrite_NoDuplicateEvent()
    {
        // Arrange — unit already has OwnerId set (previous processing succeeded)
        var dbName = $"{nameof(OL_S05_Redelivery_OwnerId_AlreadySet_NoOverwrite_NoDuplicateEvent)}_{Guid.NewGuid()}";
        await using var setupCtx = ProjectsDbFixture.NewContext(dbName);
        var project = await ProjectsDbFixture.SeedProjectAsync(setupCtx);
        var unit = await ProjectsDbFixture.SeedStructuredUnitAsync(setupCtx, project.Id, floor: 1, roomNumber: "04",
            ownerEmail: "alice@test.com");

        // Simulate a prior successful linking — set OwnerId directly via consumer
        var firstCtx = ProjectsDbFixture.NewContext(dbName);
        var firstConsumer = new OwnerLinkingConsumer(firstCtx, NullLogger<OwnerLinkingConsumer>.Instance);
        await firstConsumer.ApplyEventAsync(new UserRegisteredEvent
            { UserId = 42, Email = "alice@test.com", Role = "Owner" });
        await firstCtx.DisposeAsync();

        // Confirm first pass worked
        await using var confirmCtx = ProjectsDbFixture.NewContext(dbName);
        var linkedUnit = await confirmCtx.Units.FindAsync(unit.Id);
        Assert.Equal(42, linkedUnit!.OwnerId);
        var firstOutboxCount = confirmCtx.OutboxMessages.Count(m => m.EventType == nameof(UnitOwnerMatchedEvent));
        Assert.Equal(1, firstOutboxCount);

        // Act — re-deliver the same event
        await using var replayCtx = ProjectsDbFixture.NewContext(dbName);
        var replayConsumer = new OwnerLinkingConsumer(replayCtx, NullLogger<OwnerLinkingConsumer>.Instance);
        await replayConsumer.ApplyEventAsync(new UserRegisteredEvent
            { UserId = 42, Email = "alice@test.com", Role = "Owner" });

        // Assert — OwnerId unchanged, no additional outbox row
        await using var readCtx = ProjectsDbFixture.NewContext(dbName);
        var finalUnit = await readCtx.Units.FindAsync(unit.Id);
        Assert.Equal(42, finalUnit!.OwnerId);
        var totalOutboxCount = readCtx.OutboxMessages.Count(m => m.EventType == nameof(UnitOwnerMatchedEvent));
        Assert.Equal(1, totalOutboxCount); // still only one row
    }
}
