using Microsoft.EntityFrameworkCore;
using IoBuild.Shared.Infrastructure.EFC.Extensions;

namespace IoBuild.Analytics;

public class AnalyticsDbContext(DbContextOptions<AnalyticsDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.UseSnakeCaseNamingConvention();
    }
}
