using FluentAssertions;
using IoBuild.Projects.Application.Services;
using IoBuild.Projects.Domain.Services.Commands.Units;
using IoBuild.Projects.Infrastructure.Repositories;
using IoBuild.Projects.Tests.Infrastructure;

namespace IoBuild.Projects.Tests.Application;

/// <summary>
/// B3 [RED] — AssignUnitOwnerEmailCommand handler tests.
/// Tests lower-casing, null-clear, KeyNotFoundException, and OccupiedUnits recompute.
/// Matches existing EF-InMemory + ProjectsDbFixture style.
/// </summary>
public class AssignUnitOwnerEmailTests
{
    [Fact]
    public async Task Handle_AssignEmail_LowerCasesAndPersists()
    {
        var db = nameof(Handle_AssignEmail_LowerCasesAndPersists);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);
        var unit = await ProjectsDbFixture.SeedStructuredUnitAsync(ctx, project.Id, floor: 1, roomNumber: "01");

        var sut = BuildService(ctx);
        await sut.Handle(new AssignUnitOwnerEmailCommand(unit.Id, "Alice@EXAMPLE.COM"));

        var updated = await ctx.Units.FindAsync(unit.Id);
        updated!.OwnerEmail.Should().Be("alice@example.com", "ADR-D: emails are always lower-cased");
    }

    [Fact]
    public async Task Handle_AssignEmail_NullClears_OwnerEmail()
    {
        var db = nameof(Handle_AssignEmail_NullClears_OwnerEmail);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);
        var unit = await ProjectsDbFixture.SeedStructuredUnitAsync(ctx, project.Id, floor: 1, roomNumber: "01", ownerEmail: "someone@test.com");

        var sut = BuildService(ctx);
        await sut.Handle(new AssignUnitOwnerEmailCommand(unit.Id, null));

        var updated = await ctx.Units.FindAsync(unit.Id);
        updated!.OwnerEmail.Should().BeNull("passing null clears the email");
    }

    [Fact]
    public async Task Handle_UnknownUnit_ThrowsKeyNotFoundException()
    {
        var db = nameof(Handle_UnknownUnit_ThrowsKeyNotFoundException);
        await using var ctx = ProjectsDbFixture.NewContext(db);

        var sut = BuildService(ctx);

        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => sut.Handle(new AssignUnitOwnerEmailCommand(9999, "x@test.com")));
    }

    [Fact]
    public async Task Handle_AssignEmail_RecomputesOccupiedUnits_Increment()
    {
        var db = nameof(Handle_AssignEmail_RecomputesOccupiedUnits_Increment);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        // Seed 2 units, neither has email → OccupiedUnits = 0
        var unit1 = await ProjectsDbFixture.SeedStructuredUnitAsync(ctx, project.Id, floor: 1, roomNumber: "01");
        await ProjectsDbFixture.SeedStructuredUnitAsync(ctx, project.Id, floor: 1, roomNumber: "02");

        var sut = BuildService(ctx);
        await sut.Handle(new AssignUnitOwnerEmailCommand(unit1.Id, "bob@test.com"));

        var updatedProject = await ctx.Projects.FindAsync(project.Id);
        updatedProject!.OccupiedUnits.Should().Be(1, "one unit now has an email");
    }

    [Fact]
    public async Task Handle_ClearEmail_RecomputesOccupiedUnits_Decrement()
    {
        var db = nameof(Handle_ClearEmail_RecomputesOccupiedUnits_Decrement);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        var unit1 = await ProjectsDbFixture.SeedStructuredUnitAsync(ctx, project.Id, floor: 1, roomNumber: "01", ownerEmail: "alice@test.com");
        var unit2 = await ProjectsDbFixture.SeedStructuredUnitAsync(ctx, project.Id, floor: 1, roomNumber: "02", ownerEmail: "bob@test.com");

        // Manually set OccupiedUnits = 2 to reflect pre-condition
        project.Update(project.Name, project.Description, project.Location,
            totalUnits: 2, occupiedUnits: 2, project.Status, project.ImageUrl);
        ctx.Projects.Update(project);
        await ctx.SaveChangesAsync();

        var sut = BuildService(ctx);
        await sut.Handle(new AssignUnitOwnerEmailCommand(unit1.Id, null));

        var updatedProject = await ctx.Projects.FindAsync(project.Id);
        updatedProject!.OccupiedUnits.Should().Be(1, "clearing one email reduces occupied count");
    }

    // ------------------------------------------------------------------ //
    //  Helpers
    // ------------------------------------------------------------------ //

    private static UnitCommandService BuildService(
        IoBuild.Projects.Infrastructure.Persistence.AppDbContext ctx)
    {
        var unitRepo = new UnitRepository(ctx);
        var projectRepo = new ProjectRepository(ctx);
        var outboxRepo = new OutboxMessageRepository(ctx);
        return new UnitCommandService(unitRepo, ctx, outboxRepo, projectRepo);
    }
}
