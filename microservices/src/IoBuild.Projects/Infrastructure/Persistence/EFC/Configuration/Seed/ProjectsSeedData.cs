using Microsoft.EntityFrameworkCore;

namespace IoBuild.Projects.Infrastructure.Persistence.EFC.Configuration.Seed;

public static class ProjectsSeedData
{
    /// <summary>
    /// WU-3 seed reset: all demo projects, units, and clients removed so a fresh stack
    /// starts with 0 rows in <c>projects</c>, <c>units</c>, and <c>clients</c>.
    /// Rows previously baked into the InitialCreate migration via InsertData are removed
    /// by the <c>RemoveDemoSeed</c> EF migration.
    /// UnitOwnerAnnouncer will no-op on startup when the units table is empty
    /// (iterates zero rows — no AMQP messages published).
    /// </summary>
    public static void ApplyProjectsSeedData(this ModelBuilder builder)
    {
        // No demo seed — demo-from-zero reset (device-type-catalog WU-3).
    }
}
