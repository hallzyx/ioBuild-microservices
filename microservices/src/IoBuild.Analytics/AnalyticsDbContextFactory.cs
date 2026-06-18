using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IoBuild.Analytics;

/// <summary>
/// Design-time factory used by the EF Core tools (<c>dotnet ef migrations</c>).
/// Uses an explicit server version so the tooling never has to connect to a live
/// database to build the model. Not used at runtime (Program.cs configures the
/// real connection from environment variables).
/// </summary>
public class AnalyticsDbContextFactory : IDesignTimeDbContextFactory<AnalyticsDbContext>
{
    public AnalyticsDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AnalyticsDbContext>()
            .UseMySql(
                "Server=localhost;Database=iobuild_analytics;User=root;Password=root;",
                ServerVersion.Parse("8.0.36-mysql"))
            .Options;

        return new AnalyticsDbContext(options);
    }
}
