using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IoBuild.Projects.Infrastructure.Persistence;

/// <summary>
/// Design-time factory used by the EF Core tools (<c>dotnet ef migrations</c>).
/// Uses an explicit server version so the tooling never has to connect to a live
/// database to build the model. Not used at runtime (Program.cs configures the
/// real connection from environment variables).
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(
                "Server=localhost;Database=iobuild_projects;User=root;Password=root;",
                ServerVersion.Parse("8.0.36-mysql"))
            .Options;

        return new AppDbContext(options);
    }
}
