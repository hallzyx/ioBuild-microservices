using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Projects.Tests.Infrastructure;

/// <summary>
/// Shared test fixture for Projects EF-InMemory contexts (Task 1.2, §9.2).
/// Promotes the three-context InMemory pattern already used in
/// OutboxMessageRepositoryPersistenceTests into a reusable helper.
///
/// Caveat: EF-InMemory does NOT enforce unique indexes and uses ordinal
/// (case-sensitive) string comparison. Tests that rely on unique-constraint
/// behavior should use SQLite-in-memory instead (see §9.2 in design.md).
/// Lower-casing emails before persisting (ADR-D) makes email matching
/// deterministic on both providers.
/// </summary>
public static class ProjectsDbFixture
{
    /// <summary>
    /// Builds a fresh <see cref="AppDbContext"/> backed by an EF InMemory
    /// database with the given <paramref name="dbName"/>.
    /// All contexts sharing the same name share the same in-memory store —
    /// use distinct names per test to guarantee isolation.
    /// </summary>
    public static AppDbContext NewContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new AppDbContext(options);
    }

    // ------------------------------------------------------------------ //
    //  Seed helpers
    // ------------------------------------------------------------------ //

    /// <summary>Seeds a <see cref="Project"/> row and saves it. Returns the saved entity.</summary>
    public static async Task<Project> SeedProjectAsync(
        AppDbContext ctx,
        int builderId = 1,
        string name = "Test Project")
    {
        var project = new Project(name, "Test description", "Lima", 10, builderId, "http://img.test");
        await ctx.Projects.AddAsync(project);
        await ctx.SaveChangesAsync();
        return project;
    }

    /// <summary>Seeds a <see cref="Unit"/> row and saves it. Returns the saved entity.</summary>
    public static async Task<Unit> SeedUnitAsync(
        AppDbContext ctx,
        int projectId,
        string unitNumber = "1-01",
        int ownerId = 0)
    {
        var unit = new Unit(projectId, unitNumber, ownerId);
        await ctx.Units.AddAsync(unit);
        await ctx.SaveChangesAsync();
        return unit;
    }

    // NOTE: SeedRegisteredOwnerAsync is intentionally omitted from PR 1.
    // The RegisteredOwner entity is introduced in PR 3 (Task 3.3). Adding it
    // here before the entity exists would widen PR 1 scope beyond the agreed
    // boundary. Add the helper in PR 3 alongside the entity.
}
