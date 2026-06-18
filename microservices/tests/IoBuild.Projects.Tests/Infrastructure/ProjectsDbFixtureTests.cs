using FluentAssertions;
using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Infrastructure.Persistence;

namespace IoBuild.Projects.Tests.Infrastructure;

/// <summary>
/// Task 1.1 [RED] — Smoke test: ProjectsDbFixture.NewContext returns a usable
/// AppDbContext (InMemory) where a Unit can be saved and read back.
/// </summary>
public class ProjectsDbFixtureTests
{
    [Fact]
    public async Task NewContext_CanRoundTrip_Unit()
    {
        // Arrange
        var dbName = nameof(NewContext_CanRoundTrip_Unit);
        await using var writeCtx = ProjectsDbFixture.NewContext(dbName);

        var unit = new Unit(projectId: 1, unitNumber: "5-01", ownerId: 99);
        await writeCtx.Units.AddAsync(unit);
        await writeCtx.SaveChangesAsync();
        var savedId = unit.Id;

        // Act — read back in a fresh context (same InMemory store, different scope)
        await using var readCtx = ProjectsDbFixture.NewContext(dbName);
        var loaded = await readCtx.Units.FindAsync(savedId);

        // Assert
        loaded.Should().NotBeNull();
        loaded!.UnitNumber.Should().Be("5-01");
    }
}
