using Microsoft.EntityFrameworkCore;

namespace IoBuild.IAM.Infrastructure.Persistence.EFC.Configuration.Seed;

public static class IamSeedData
{
    /// <summary>
    /// WU-3 seed reset: demo users removed so a fresh stack starts with 0 rows in `users`.
    /// The two demo rows (builder@iobuilt.com / owner@iobuilt.com) that were seeded via
    /// HasData in AddOutbox migration are deleted by the RemoveDemoSeed migration.
    /// Builder and owner now register themselves through the normal IAM flow.
    /// </summary>
    public static void ApplyIamSeedData(this ModelBuilder builder)
    {
        // No demo seed — demo-from-zero reset (device-type-catalog WU-3).
    }
}
