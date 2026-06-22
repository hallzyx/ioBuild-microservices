using FluentAssertions;
using IoBuild.Projects.Application.Services;
using IoBuild.Projects.Domain.Services.Commands.Projects;
using IoBuild.Projects.Tests.Infrastructure;

namespace IoBuild.Projects.Tests.Application;

/// <summary>
/// T-C1 [RED] — Validation tests for DefineStructureResource / ProjectStructureCommandService.
/// Spec: S2.3, S2.4 — unknown type codes and out-of-range floor references return ArgumentException.
/// </summary>
public class DefineStructureValidationTests
{
    // ─── SC-2.3: unknown device type code → ArgumentException ───────────────

    [Fact]
    public async Task Handle_UnknownDeviceTypeCode_ThrowsArgumentException()
    {
        var db = nameof(Handle_UnknownDeviceTypeCode_ThrowsArgumentException);
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
                    DeviceTypes: new List<string> { "SmartMeter", "LaserCannon" })
            },
            BuilderId: 1);

        var act = async () => await sut.Handle(command);

        await act.Should().ThrowAsync<ArgumentException>(
            "unknown device type 'LaserCannon' must be rejected with ArgumentException (SC-2.3 / HTTP 400)");
    }

    // ─── Triangulation: multiple unknown types also rejected ────────────────

    [Fact]
    public async Task Handle_MultipleUnknownDeviceTypeCodes_ThrowsArgumentException()
    {
        var db = nameof(Handle_MultipleUnknownDeviceTypeCodes_ThrowsArgumentException);
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
                    DeviceTypes: new List<string> { "LaserCannon", "TurbineEngine" })
            },
            BuilderId: 1);

        var act = async () => await sut.Handle(command);

        await act.Should().ThrowAsync<ArgumentException>(
            "all unknown device type codes must be rejected");
    }

    // ─── SC-2.3 triangulation: known codes do NOT throw ─────────────────────

    [Fact]
    public async Task Handle_AllKnownDeviceTypeCodes_DoesNotThrow()
    {
        var db = nameof(Handle_AllKnownDeviceTypeCodes_DoesNotThrow);
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
                    DeviceTypes: new List<string> { "SmartMeter", "WaterSensor", "SmokeDetector" })
            },
            BuilderId: 1);

        var act = async () => await sut.Handle(command);

        await act.Should().NotThrowAsync(
            "all three known device types are valid and must not be rejected");
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
}
