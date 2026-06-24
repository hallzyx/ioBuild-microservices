using Microsoft.EntityFrameworkCore;

namespace IoBuild.Analytics.Infrastructure.Persistence.EFC.Configuration.Seed;

/// <summary>
/// WU-3 seed reset: demo metric rows removed.
///
/// NOTE: This class is intentionally empty. AnalyticsDbContext does NOT call
/// ApplyAnalyticsSeedData — it never did after ADR-6 refactored BuilderMetrics and
/// OwnerMetrics from persisted tables to in-process computed DTOs. The InitialCreate
/// migration contains no InsertData for metrics. There is nothing to delete.
///
/// No RemoveDemoSeed migration is needed for Analytics.
/// </summary>
public static class AnalyticsSeedData
{
    public static void ApplyAnalyticsSeedData(this ModelBuilder builder)
    {
        // No-op. See class doc comment.
    }
}
