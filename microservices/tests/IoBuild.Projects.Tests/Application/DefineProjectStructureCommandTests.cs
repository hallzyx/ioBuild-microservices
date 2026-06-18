using System.Text.Json;
using FluentAssertions;
using IoBuild.Projects.Application.Services;
using IoBuild.Projects.Domain.Services.Commands.Projects;
using IoBuild.Projects.Infrastructure.Persistence;
using IoBuild.Projects.Infrastructure.Repositories;
using IoBuild.Projects.Tests.Infrastructure;
using IoBuild.Shared.Domain.Model.Events;

namespace IoBuild.Projects.Tests.Application;

/// <summary>
/// Task 3.7 [RED] — DefineProjectStructureCommand service tests.
/// All tests FAIL until Tasks 3.8–3.9 implement the command and service.
/// Uses real EF-InMemory contexts (ProjectsDbFixture) for authentic persistence
/// assertions (§9.2 — promotes the InMemory fixture pattern).
/// </summary>
public class DefineProjectStructureCommandTests
{
    // ------------------------------------------------------------------ //
    //  PS-S01 / REQ-PS-02 — Happy path: correct unit count and fields
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Handle_DefineStructure_CreatesCorrectUnitCount()
    {
        // Arrange
        var db = nameof(Handle_DefineStructure_CreatesCorrectUnitCount);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        var sut = BuildService(ctx);
        var command = BuildCommand(project.Id, floors: 3, unitsPerFloor: 4);

        // Act
        await sut.Handle(command);

        // Assert: 3 × 4 = 12 units
        ctx.Units.Count(u => u.ProjectId == project.Id).Should().Be(12);
    }

    [Fact]
    public async Task Handle_DefineStructure_UnitsHaveCorrectFloorAndRoomNumber()
    {
        var db = nameof(Handle_DefineStructure_UnitsHaveCorrectFloorAndRoomNumber);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        var sut = BuildService(ctx);
        var command = BuildCommand(project.Id, floors: 2, unitsPerFloor: 3);

        await sut.Handle(command);

        var units = ctx.Units.Where(u => u.ProjectId == project.Id).OrderBy(u => u.Floor).ThenBy(u => u.RoomNumber).ToList();
        units.Should().Contain(u => u.Floor == 1 && u.RoomNumber == "01");
        units.Should().Contain(u => u.Floor == 1 && u.RoomNumber == "02");
        units.Should().Contain(u => u.Floor == 1 && u.RoomNumber == "03");
        units.Should().Contain(u => u.Floor == 2 && u.RoomNumber == "01");
        units.Should().Contain(u => u.Floor == 2 && u.RoomNumber == "02");
        units.Should().Contain(u => u.Floor == 2 && u.RoomNumber == "03");
    }

    [Fact]
    public async Task Handle_DefineStructure_UnitsHaveNullOwnerId()
    {
        // REQ-PS-02: OwnerId is null at creation
        var db = nameof(Handle_DefineStructure_UnitsHaveNullOwnerId);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        var sut = BuildService(ctx);
        var command = BuildCommand(project.Id, floors: 1, unitsPerFloor: 2);

        await sut.Handle(command);

        ctx.Units.Where(u => u.ProjectId == project.Id)
            .All(u => u.OwnerId == null)
            .Should().BeTrue("newly created structural units have no owner");
    }

    // ------------------------------------------------------------------ //
    //  PS-S06 / REQ-PS-04 — UnitCreatedEvent carries real UnitId (non-zero)
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Handle_DefineStructure_OutboxUnitCreatedEvents_HaveRealUnitId()
    {
        var db = nameof(Handle_DefineStructure_OutboxUnitCreatedEvents_HaveRealUnitId);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 7);

        var sut = BuildService(ctx);
        var command = BuildCommand(project.Id, floors: 1, unitsPerFloor: 2);

        await sut.Handle(command);

        var unitEvents = ctx.OutboxMessages
            .Where(m => m.EventType == nameof(UnitCreatedEvent))
            .ToList();

        unitEvents.Should().HaveCount(2, "one UnitCreatedEvent per unit");
        foreach (var msg in unitEvents)
        {
            var evt = JsonSerializer.Deserialize<UnitCreatedEvent>(msg.Payload)!;
            evt.UnitId.Should().BeGreaterThan(0, "UnitId must carry the real DB-assigned id (ADR-A / REQ-PS-04)");
            evt.Floor.Should().Be(1);
        }
    }

    // ------------------------------------------------------------------ //
    //  PS-S01 / REQ-PS-05 — FloorStructureDefinedEvent per floor
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Handle_DefineStructure_WritesOneFloorEventPerFloor()
    {
        var db = nameof(Handle_DefineStructure_WritesOneFloorEventPerFloor);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 5);

        var sut = BuildService(ctx);
        var command = BuildCommand(project.Id, floors: 3, unitsPerFloor: 4, builderId: 5);

        await sut.Handle(command);

        var floorEvents = ctx.OutboxMessages
            .Where(m => m.EventType == nameof(FloorStructureDefinedEvent))
            .ToList();

        floorEvents.Should().HaveCount(3, "one FloorStructureDefinedEvent per floor (REQ-PS-05)");
        foreach (var msg in floorEvents)
        {
            var evt = JsonSerializer.Deserialize<FloorStructureDefinedEvent>(msg.Payload)!;
            evt.ProjectId.Should().Be(project.Id);
            evt.UnitCount.Should().Be(4);
        }
        var floors = floorEvents
            .Select(m => JsonSerializer.Deserialize<FloorStructureDefinedEvent>(m.Payload)!.Floor)
            .OrderBy(f => f)
            .ToList();
        floors.Should().Equal(1, 2, 3);
    }

    // ------------------------------------------------------------------ //
    //  PS-S05 / REQ-PS-03 — 409 Conflict when structure already defined
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Handle_DefineStructure_Throws409_WhenProjectAlreadyHasUnits()
    {
        var db = nameof(Handle_DefineStructure_Throws409_WhenProjectAlreadyHasUnits);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        // Pre-seed a unit so the project already has units
        await ProjectsDbFixture.SeedStructuredUnitAsync(ctx, project.Id, floor: 1, roomNumber: "01");

        var sut = BuildService(ctx);
        var command = BuildCommand(project.Id, floors: 2, unitsPerFloor: 3);

        // Act & Assert — service must throw an appropriate exception
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => sut.Handle(command));

        // No extra units should have been created
        ctx.Units.Count(u => u.ProjectId == project.Id).Should().Be(1);
    }

    // ------------------------------------------------------------------ //
    //  PS-S03/PS-S04 / REQ-PS-01 — Validation: floors < 1 or unitsPerFloor < 1
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Handle_DefineStructure_Throws422_WhenFloorsIsZero()
    {
        var db = nameof(Handle_DefineStructure_Throws422_WhenFloorsIsZero);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        var sut = BuildService(ctx);
        var command = BuildCommand(project.Id, floors: 0, unitsPerFloor: 4);

        await Assert.ThrowsAsync<ArgumentException>(() => sut.Handle(command));

        ctx.Units.Count(u => u.ProjectId == project.Id).Should().Be(0, "no units created on validation failure");
    }

    [Fact]
    public async Task Handle_DefineStructure_Throws422_WhenUnitsPerFloorIsZero()
    {
        var db = nameof(Handle_DefineStructure_Throws422_WhenUnitsPerFloorIsZero);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        var sut = BuildService(ctx);
        var command = BuildCommand(project.Id, floors: 3, unitsPerFloor: 0);

        await Assert.ThrowsAsync<ArgumentException>(() => sut.Handle(command));

        ctx.Units.Count(u => u.ProjectId == project.Id).Should().Be(0);
    }

    // ------------------------------------------------------------------ //
    //  Unit-first owner match (§3.3): if RegisteredOwner row exists for email,
    //  OwnerId is set immediately and UnitOwnerMatchedEvent is written.
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task Handle_DefineStructure_UnitFirstMatch_SetsOwnerIdImmediately()
    {
        var db = nameof(Handle_DefineStructure_UnitFirstMatch_SetsOwnerIdImmediately);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        // Pre-seed a RegisteredOwner row so the unit-first path can match
        await ProjectsDbFixture.SeedRegisteredOwnerAsync(ctx, email: "alice@test.com", userId: 42);

        var sut = BuildService(ctx);

        // Define 1 floor with 1 unit, pre-assigning alice@test.com
        var command = new DefineProjectStructureCommand(
            ProjectId: project.Id,
            Floors: new List<FloorSpec>
            {
                new FloorSpec(Floor: 1, Rooms: new List<RoomSpec>
                {
                    new RoomSpec(RoomNumber: "01", OwnerEmail: "alice@test.com")
                })
            },
            BuilderId: 1);

        await sut.Handle(command);

        // Assert: unit has OwnerId set immediately (unit-first match)
        var unit = ctx.Units.Single(u => u.ProjectId == project.Id);
        unit.OwnerId.Should().Be(42, "RegisteredOwner row existed → OwnerId set inline (§3.3)");
        unit.OwnerEmail.Should().Be("alice@test.com");

        // Assert: UnitOwnerMatchedEvent outbox row written
        var matchedEvents = ctx.OutboxMessages
            .Where(m => m.EventType == nameof(UnitOwnerMatchedEvent))
            .ToList();
        matchedEvents.Should().HaveCount(1, "one UnitOwnerMatchedEvent for the matched unit");
        var matchedEvt = JsonSerializer.Deserialize<UnitOwnerMatchedEvent>(matchedEvents[0].Payload)!;
        matchedEvt.OwnerUserId.Should().Be(42);
        matchedEvt.OwnerEmail.Should().Be("alice@test.com");
    }

    [Fact]
    public async Task Handle_DefineStructure_UnitFirstMatch_NullEmailDoesNotMatch()
    {
        // Units without OwnerEmail should not attempt a RegisteredOwner lookup
        var db = nameof(Handle_DefineStructure_UnitFirstMatch_NullEmailDoesNotMatch);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        var sut = BuildService(ctx);
        var command = BuildCommand(project.Id, floors: 1, unitsPerFloor: 2);

        await sut.Handle(command);

        ctx.OutboxMessages.Any(m => m.EventType == nameof(UnitOwnerMatchedEvent))
            .Should().BeFalse("no UnitOwnerMatchedEvent when no owner email is assigned");
    }

    // ------------------------------------------------------------------ //
    //  Helpers
    // ------------------------------------------------------------------ //

    private static ProjectStructureCommandService BuildService(
        IoBuild.Projects.Infrastructure.Persistence.AppDbContext ctx)
    {
        var unitRepo = new UnitRepository(ctx);
        var projectRepo = new ProjectRepository(ctx);
        var outboxRepo = new OutboxMessageRepository(ctx);
        return new ProjectStructureCommandService(unitRepo, projectRepo, outboxRepo, ctx);
    }

    /// <summary>
    /// Builds a uniform DefineProjectStructureCommand with room numbers "01".."NN".
    /// </summary>
    private static DefineProjectStructureCommand BuildCommand(
        int projectId,
        int floors,
        int unitsPerFloor,
        int builderId = 1)
    {
        var floorSpecs = Enumerable.Range(1, floors)
            .Select(f => new FloorSpec(
                Floor: f,
                Rooms: Enumerable.Range(1, unitsPerFloor)
                    .Select(r => new RoomSpec(RoomNumber: r.ToString("D2"), OwnerEmail: null))
                    .ToList()))
            .ToList();

        return new DefineProjectStructureCommand(
            ProjectId: projectId,
            Floors: floorSpecs,
            BuilderId: builderId);
    }
}
