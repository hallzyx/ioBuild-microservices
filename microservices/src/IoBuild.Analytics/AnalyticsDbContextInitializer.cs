using Microsoft.EntityFrameworkCore;

namespace IoBuild.Analytics;

public class AnalyticsDbContextInitializer(AnalyticsDbContext context, ILogger<AnalyticsDbContextInitializer> logger)
{
    public async Task InitializeAsync()
    {
        try
        {
            if (context.Database.IsRelational())
            {
                // Apply EF Core migrations. Unlike EnsureCreated, Migrate() applies
                // incremental schema changes to an existing database, so later schema
                // changes (new tables/columns) are picked up without a volume reset.
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migrated/verified successfully.");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database migration skipped or failed. The service will continue without a database.");
        }
    }
}
