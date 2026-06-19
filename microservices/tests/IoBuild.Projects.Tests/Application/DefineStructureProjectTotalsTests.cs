using FluentAssertions;
using IoBuild.Projects.Application.Services;
using IoBuild.Projects.Domain.Services.Commands.Projects;
using IoBuild.Projects.Infrastructure.Repositories;
using IoBuild.Projects.Tests.Infrastructure;

namespace IoBuild.Projects.Tests.Application;

/// <summary>
/// B2 [RED] — Project totals sync on DefineProjectStructure.
/// Asserts that after calling ProjectStructureCommandService.Handle,
/// the parent Project aggregate has TotalUnits == N and
/// OccupiedUnits == count(units where OwnerEmail != null).
/// OwnerId is NOT used for occupied count — a pre-assigned email counts as occupied
/// even if OwnerId is still null (async owner-linking may come later).
/// </summary>
public class DefineStructureProjectTotalsTests
{
    [Fact]
    public async Task Handle_DefineStructure_SetsProjectTotalUnits()
    {
        var db = nameof(Handle_DefineStructure_SetsProjectTotalUnits);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        var sut = BuildService(ctx);
        var command = BuildCommand(project.Id, floors: 3, unitsPerFloor: 4);

        await sut.Handle(command);

        // Re-fetch to see persisted state
        var updated = await ctx.Projects.FindAsync(project.Id);
        updated!.TotalUnits.Should().Be(12, "3 floors × 4 units = 12");
    }

    [Fact]
    public async Task Handle_DefineStructure_SetsOccupiedUnits_BasedOnOwnerEmail()
    {
        var db = nameof(Handle_DefineStructure_SetsOccupiedUnits_BasedOnOwnerEmail);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        // 2 units with ownerEmail, 1 without
        var command = new DefineProjectStructureCommand(
            ProjectId: project.Id,
            Floors: new List<FloorSpec>
            {
                new FloorSpec(Floor: 1, Rooms: new List<RoomSpec>
                {
                    new RoomSpec(RoomNumber: "01", OwnerEmail: "alice@test.com"),
                    new RoomSpec(RoomNumber: "02", OwnerEmail: "bob@test.com"),
                    new RoomSpec(RoomNumber: "03", OwnerEmail: null)
                })
            },
            BuilderId: 1);

        await sut(ctx).Handle(command);

        var updated = await ctx.Projects.FindAsync(project.Id);
        updated!.TotalUnits.Should().Be(3, "3 rooms defined");
        updated.OccupiedUnits.Should().Be(2, "2 of 3 rooms have an OwnerEmail pre-assigned");
    }

    [Fact]
    public async Task Handle_DefineStructure_OccupiedUnits_Zero_WhenNoEmailsAssigned()
    {
        var db = nameof(Handle_DefineStructure_OccupiedUnits_Zero_WhenNoEmailsAssigned);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        var sut = BuildService(ctx);
        var command = BuildCommand(project.Id, floors: 2, unitsPerFloor: 3);

        await sut.Handle(command);

        var updated = await ctx.Projects.FindAsync(project.Id);
        updated!.OccupiedUnits.Should().Be(0, "no emails assigned → zero occupied");
    }

    [Fact]
    public async Task Handle_DefineStructure_OccupiedUnits_CountsEmailNotOwnerId()
    {
        // OwnerId is null at creation time; unit-first matching sets it but the
        // OccupiedUnits count must use OwnerEmail presence, not OwnerId.
        var db = nameof(Handle_DefineStructure_OccupiedUnits_CountsEmailNotOwnerId);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        // Seed a RegisteredOwner so the unit-first path sets OwnerId
        await ProjectsDbFixture.SeedRegisteredOwnerAsync(ctx, email: "carol@test.com", userId: 99);

        var command = new DefineProjectStructureCommand(
            ProjectId: project.Id,
            Floors: new List<FloorSpec>
            {
                new FloorSpec(Floor: 1, Rooms: new List<RoomSpec>
                {
                    new RoomSpec(RoomNumber: "01", OwnerEmail: "carol@test.com"),
                    new RoomSpec(RoomNumber: "02", OwnerEmail: null)
                })
            },
            BuilderId: 1);

        await BuildService(ctx).Handle(command);

        var updated = await ctx.Projects.FindAsync(project.Id);
        updated!.OccupiedUnits.Should().Be(1,
            "OccupiedUnits is count of units with OwnerEmail != null; OwnerId state is irrelevant");
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

    private static ProjectStructureCommandService sut(
        IoBuild.Projects.Infrastructure.Persistence.AppDbContext ctx) => BuildService(ctx);

    private static DefineProjectStructureCommand BuildCommand(
        int projectId, int floors, int unitsPerFloor, int builderId = 1)
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
