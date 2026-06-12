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
                // Use EnsureCreated instead of MigrateAsync since we use HasData for seed
                // and don't maintain EF Core migration files
                await context.Database.EnsureCreatedAsync();
                logger.LogInformation("Database created/verified successfully.");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Database migration skipped or failed. The service will continue without a database.");
        }
    }
}
