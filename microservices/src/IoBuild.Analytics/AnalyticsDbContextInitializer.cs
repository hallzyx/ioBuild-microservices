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
                await context.Database.MigrateAsync();
                logger.LogInformation("Database migration completed successfully.");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database migration skipped or failed. The service will continue without a database.");
        }
    }
}
