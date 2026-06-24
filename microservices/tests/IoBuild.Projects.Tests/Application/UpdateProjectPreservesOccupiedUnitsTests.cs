using FluentAssertions;
using IoBuild.Projects.Application.Services;
using IoBuild.Projects.Domain.Model.ValueObjects;
using IoBuild.Projects.Domain.Services.Commands.Projects;
using IoBuild.Projects.Infrastructure.Repositories;
using IoBuild.Projects.Tests.Infrastructure;

namespace IoBuild.Projects.Tests.Application;

/// <summary>
/// Regression: editing a project must NOT clobber the derived OccupiedUnits counter.
///
/// OccupiedUnits is recomputed from actual unit ownership by
/// <see cref="UnitCommandService"/> (AssignUnitOwnerEmailCommand). The project edit
/// form used to expose OccupiedUnits as an editable field; saving it sent a stale
/// value that overwrote the real count, making the header read "0 occupied" as if
/// the owner had vanished. The update handler must preserve the existing value.
/// </summary>
public class UpdateProjectPreservesOccupiedUnitsTests
{
    [Fact]
    public async Task Handle_Update_PreservesExistingOccupiedUnits()
    {
        var db = nameof(Handle_Update_PreservesExistingOccupiedUnits);
        await using var ctx = ProjectsDbFixture.NewContext(db);
        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        // Simulate the owner-assignment flow having set OccupiedUnits = 2.
        project.Update(project.Name, project.Description, project.Location,
            totalUnits: 9, occupiedUnits: 2, project.Status, project.ImageUrl);
        ctx.Projects.Update(project);
        await ctx.SaveChangesAsync();

        var sut = BuildService(ctx);

        // Edit metadata, passing STALE unit counts (what a stale form payload sends).
        await sut.Handle(new UpdateProjectCommand(
            id: project.Id,
            name: "Renamed",
            description: project.Description,
            location: project.Location,
            totalUnits: 999,
            occupiedUnits: 0,
            status: project.Status,
            imageUrl: project.ImageUrl));

        var updated = await ctx.Projects.FindAsync(project.Id);
        updated!.Name.Should().Be("Renamed", "metadata edits still apply");
        updated.OccupiedUnits.Should().Be(2,
            "OccupiedUnits is derived from unit ownership and must not be overwritten by the edit form");
        updated.TotalUnits.Should().Be(9,
            "TotalUnits is set by structure definition and must not be overwritten by the edit form");
    }

    private static ProjectCommandService BuildService(
        IoBuild.Projects.Infrastructure.Persistence.AppDbContext ctx)
    {
        var projectRepo = new ProjectRepository(ctx);
        var outboxRepo = new OutboxMessageRepository(ctx);
        return new ProjectCommandService(projectRepo, ctx, outboxRepo);
    }
}
