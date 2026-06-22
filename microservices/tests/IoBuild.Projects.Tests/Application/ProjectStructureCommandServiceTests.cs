using System.Text.Json;
using FluentAssertions;
using IoBuild.Projects.Application.Services;
using IoBuild.Projects.Domain.Services.Commands.Projects;
using IoBuild.Projects.Tests.Infrastructure;
using IoBuild.Shared.Domain.Model.Events;

namespace IoBuild.Projects.Tests.Application;

/// <summary>
/// T-C1 [RED] — ProjectStructureCommandService: per-floor DeviceTypes mapping.
/// Spec: S2.1–S2.6.
/// All new tests MUST fail until T-C2/T-C3 implement the changes.
/// Existing tests in DefineProjectStructureCommandTests MUST continue to compile and pass.
/// </summary>
public class ProjectStructureCommandServiceTests
{
    // ─── SC-2.1: per-floor selection carried into FloorStructureDefinedEvent ──

    [Fact]
    public async Task Handle_PerFloorDeviceTypes_Floor1GetsSelection_Floor2GetsNull()
    {
        // Arrange
        var db = nameof(Handle_PerFloorDeviceTypes_Floor1GetsSelection_Floor2GetsNull);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        var sut = BuildService(ctx);

        // 2 floors; floor 1 selects SmartMeter+SmokeDetector; floor 2 → null → consumer uses defaults
        var command = new DefineProjectStructureCommand(
            ProjectId: project.Id,
            Floors: new List<FloorSpec>
            {
                new FloorSpec(
                    Floor: 1,
                    Rooms: new List<RoomSpec> { new RoomSpec("01", null) },
                    DeviceTypes: new List<string> { "SmartMeter", "SmokeDetector" }),
                new FloorSpec(
                    Floor: 2,
                    Rooms: new List<RoomSpec> { new RoomSpec("01", null) },
                    DeviceTypes: null)
            },
            BuilderId: 1);

        // Act
        await sut.Handle(command);

        // Assert — materialize first so JsonSerializer doesn't enter EF expression tree
        var rawMessages = ctx.OutboxMessages
            .Where(m => m.EventType == nameof(FloorStructureDefinedEvent))
            .ToList();
        var floorEvents = rawMessages
            .Select(m => JsonSerializer.Deserialize<FloorStructureDefinedEvent>(m.Payload)!)
            .OrderBy(e => e.Floor)
            .ToList();

        floorEvents.Should().HaveCount(2);

        var floor1Evt = floorEvents.Single(e => e.Floor == 1);
        floor1Evt.DeviceTypes.Should().NotBeNull();
        floor1Evt.DeviceTypes!.Should().BeEquivalentTo(new[] { "SmartMeter", "SmokeDetector" });

        var floor2Evt = floorEvents.Single(e => e.Floor == 2);
        // null passed through → consumer resolves legacy defaults
        floor2Evt.DeviceTypes.Should().BeNull(
            "floor 2 had no selection; the service emits null so the consumer uses legacy defaults (SC-2.1)");
    }

    // ─── SC-2.2: null floorDeviceTypes → all floors emit DeviceTypes = null ──

    [Fact]
    public async Task Handle_NullDeviceTypes_AllFloorEventsHaveNullDeviceTypes()
    {
        var db = nameof(Handle_NullDeviceTypes_AllFloorEventsHaveNullDeviceTypes);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        var sut = BuildService(ctx);
        // BuildCommand uses FloorSpec with no DeviceTypes — defaults to null
        var command = BuildCommand(project.Id, floors: 2, unitsPerFloor: 2);

        await sut.Handle(command);

        var rawMessages = ctx.OutboxMessages
            .Where(m => m.EventType == nameof(FloorStructureDefinedEvent))
            .ToList();
        var floorEvents = rawMessages
            .Select(m => JsonSerializer.Deserialize<FloorStructureDefinedEvent>(m.Payload)!)
            .ToList();

        floorEvents.Should().HaveCount(2);
        foreach (var e in floorEvents)
        {
            e.DeviceTypes.Should().BeNull(
                "no device type selection means the event DeviceTypes is null; consumer falls back to defaults (SC-2.2)");
        }
    }

    // ─── SC-2.5: duplicate type codes deduplicated (no error) ──────────────

    [Fact]
    public async Task Handle_DuplicateDeviceTypeCodes_AreDeduplicatedSilently()
    {
        var db = nameof(Handle_DuplicateDeviceTypeCodes_AreDeduplicatedSilently);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        var sut = BuildService(ctx);
        var command = new DefineProjectStructureCommand(
            ProjectId: project.Id,
            Floors: new List<FloorSpec>
            {
                new FloorSpec(
                    Floor: 1,
                    Rooms: new List<RoomSpec> { new RoomSpec("01", null) },
                    DeviceTypes: new List<string> { "SmartMeter", "SmartMeter", "WaterSensor" })
            },
            BuilderId: 1);

        await sut.Handle(command);

        var rawMessages = ctx.OutboxMessages
            .Where(m => m.EventType == nameof(FloorStructureDefinedEvent))
            .ToList();
        var floorEvt = rawMessages
            .Select(m => JsonSerializer.Deserialize<FloorStructureDefinedEvent>(m.Payload)!)
            .Single();

        floorEvt.DeviceTypes.Should().NotBeNull();
        floorEvt.DeviceTypes!.Should().HaveCount(2, "SmartMeter was duplicated and must be deduped (SC-2.5)");
        floorEvt.DeviceTypes.Should().Contain("SmartMeter").And.Contain("WaterSensor");
    }

    // ─── SC-2.6: empty deviceTypes array → treated as no selection → event DeviceTypes null ──

    [Fact]
    public async Task Handle_EmptyDeviceTypesArray_TreatedAsDefaults()
    {
        var db = nameof(Handle_EmptyDeviceTypesArray_TreatedAsDefaults);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        var sut = BuildService(ctx);
        var command = new DefineProjectStructureCommand(
            ProjectId: project.Id,
            Floors: new List<FloorSpec>
            {
                new FloorSpec(
                    Floor: 1,
                    Rooms: new List<RoomSpec> { new RoomSpec("01", null) },
                    DeviceTypes: new List<string>())  // explicitly empty
            },
            BuilderId: 1);

        await sut.Handle(command);

        var rawMessages = ctx.OutboxMessages
            .Where(m => m.EventType == nameof(FloorStructureDefinedEvent))
            .ToList();
        var floorEvt = rawMessages
            .Select(m => JsonSerializer.Deserialize<FloorStructureDefinedEvent>(m.Payload)!)
            .Single();

        // Design: empty list → normalize to null; consumer applies legacy defaults (SC-2.6)
        floorEvt.DeviceTypes.Should().BeNull(
            "empty device type list is treated as 'no selection'; service normalizes to null (SC-2.6)");
    }

    // ─── Regression: existing call sites without DeviceTypes still work ──────

    [Fact]
    public async Task Handle_ExistingCallSite_WithNoDeviceTypes_StillWorks()
    {
        var db = nameof(Handle_ExistingCallSite_WithNoDeviceTypes_StillWorks);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        var sut = BuildService(ctx);
        var command = BuildCommand(project.Id, floors: 1, unitsPerFloor: 2);

        var act = async () => await sut.Handle(command);
        await act.Should().NotThrowAsync("existing call sites without DeviceTypes must still work (regression guard)");

        ctx.Units.Count(u => u.ProjectId == project.Id).Should().Be(2);
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────

    private static ProjectStructureCommandService BuildService(
        IoBuild.Projects.Infrastructure.Persistence.AppDbContext ctx)
    {
        var unitRepo = new IoBuild.Projects.Infrastructure.Repositories.UnitRepository(ctx);
        var projectRepo = new IoBuild.Projects.Infrastructure.Repositories.ProjectRepository(ctx);
        var outboxRepo = new IoBuild.Projects.Infrastructure.Repositories.OutboxMessageRepository(ctx);
        return new ProjectStructureCommandService(unitRepo, projectRepo, outboxRepo, ctx);
    }

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
                    .Select(r => new RoomSpec(r.ToString("D2"), null))
                    .ToList()))
            .ToList();

        return new DefineProjectStructureCommand(
            ProjectId: projectId,
            Floors: floorSpecs,
            BuilderId: builderId);
    }
}
