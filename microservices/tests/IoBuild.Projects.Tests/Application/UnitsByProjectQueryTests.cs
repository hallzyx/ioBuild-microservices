using FluentAssertions;
using IoBuild.Projects.Application.Services;
using IoBuild.Projects.Domain.Services.Queries.Units;
using IoBuild.Projects.Infrastructure.Repositories;
using IoBuild.Projects.Tests.Infrastructure;

namespace IoBuild.Projects.Tests.Application;

/// <summary>
/// B1 [RED] — GetUnitsByProjectIdQuery tests.
/// Exercises IUnitRepository.FindByProjectIdAsync and IUnitQueryService.Handle overload.
/// Uses EF InMemory via ProjectsDbFixture (matches existing test style).
/// </summary>
public class UnitsByProjectQueryTests
{
    // ------------------------------------------------------------------ //
    //  Happy path — returns only units for the requested project
    // ------------------------------------------------------------------ //

    [Fact]
    public async Task FindByProjectId_ReturnsOnlyUnitsForThatProject()
    {
        var db = nameof(FindByProjectId_ReturnsOnlyUnitsForThatProject);
        await using var ctx = ProjectsDbFixture.NewContext(db);

        var projectA = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1, name: "Project A");
        var projectB = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1, name: "Project B");

        await ProjectsDbFixture.SeedStructuredUnitAsync(ctx, projectA.Id, floor: 1, roomNumber: "01");
        await ProjectsDbFixture.SeedStructuredUnitAsync(ctx, projectA.Id, floor: 1, roomNumber: "02");
        await ProjectsDbFixture.SeedStructuredUnitAsync(ctx, projectB.Id, floor: 1, roomNumber: "01");

        var repo = new UnitRepository(ctx);
        var result = await repo.FindByProjectIdAsync(projectA.Id);

        result.Should().HaveCount(2, "only units for projectA should be returned");
        result.Should().OnlyContain(u => u.ProjectId == projectA.Id);
    }

    [Fact]
    public async Task FindByProjectId_ReturnsEmpty_WhenProjectHasNoUnits()
    {
        var db = nameof(FindByProjectId_ReturnsEmpty_WhenProjectHasNoUnits);
        await using var ctx = ProjectsDbFixture.NewContext(db);

        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);

        var repo = new UnitRepository(ctx);
        var result = await repo.FindByProjectIdAsync(project.Id);

        result.Should().BeEmpty("a project with no units returns an empty list");
    }

    [Fact]
    public async Task QueryService_Handle_GetUnitsByProjectId_ReturnsSameResult()
    {
        var db = nameof(QueryService_Handle_GetUnitsByProjectId_ReturnsSameResult);
        await using var ctx = ProjectsDbFixture.NewContext(db);

        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 2);
        await ProjectsDbFixture.SeedStructuredUnitAsync(ctx, project.Id, floor: 2, roomNumber: "01", ownerEmail: "x@test.com");
        await ProjectsDbFixture.SeedStructuredUnitAsync(ctx, project.Id, floor: 2, roomNumber: "02");

        var sut = new UnitQueryService(new UnitRepository(ctx));
        var result = await sut.Handle(new GetUnitsByProjectIdQuery(project.Id));

        result.Should().HaveCount(2);
        result.Should().OnlyContain(u => u.ProjectId == project.Id);
    }

    [Fact]
    public async Task QueryService_Handle_GetUnitsByProjectId_ExposesOwnerEmail()
    {
        var db = nameof(QueryService_Handle_GetUnitsByProjectId_ExposesOwnerEmail);
        await using var ctx = ProjectsDbFixture.NewContext(db);

        var project = await ProjectsDbFixture.SeedProjectAsync(ctx, builderId: 1);
        await ProjectsDbFixture.SeedStructuredUnitAsync(ctx, project.Id, floor: 1, roomNumber: "01", ownerEmail: "Alice@Test.COM");

        var sut = new UnitQueryService(new UnitRepository(ctx));
        var result = (await sut.Handle(new GetUnitsByProjectIdQuery(project.Id))).ToList();

        result.Should().HaveCount(1);
        result[0].OwnerEmail.Should().Be("alice@test.com", "emails are lower-cased at creation (ADR-D)");
    }
}
