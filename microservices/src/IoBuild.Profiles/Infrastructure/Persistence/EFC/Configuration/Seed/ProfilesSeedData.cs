using Microsoft.EntityFrameworkCore;

namespace IoBuild.Profiles.Infrastructure.Persistence.EFC.Configuration.Seed;

public static class ProfilesSeedData
{
    /// <summary>
    /// WU-3 seed reset: demo profile rows (UserId=1 builder, UserId=2 owner) removed.
    /// A fresh stack starts with 0 rows in <c>profiles</c>.
    ///
    /// IMPORTANT — EnsureCreated stale-volume trap:
    /// EnsureCreated never re-runs once the schema exists, so removed rows here WILL survive
    /// on an existing Docker volume. To fully reset to demo-from-zero, the operator MUST run:
    ///   docker compose down -v
    /// before the first start of this slice. Established convention — see memory:
    /// "Analytics EnsureCreated schema trap".
    /// </summary>
    public static void ApplyProfilesSeedData(this ModelBuilder builder)
    {
        // No demo seed — demo-from-zero reset (device-type-catalog WU-3).
    }
}
