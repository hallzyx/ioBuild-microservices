using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IoBuild.IAM.Infrastructure.Persistence.EFC;

/// <summary>
/// Design-time factory used by the EF Core tools (<c>dotnet ef migrations</c>).
/// Uses an explicit server version so the tooling never has to connect to a live
/// database to build the model. Not used at runtime (Program.cs configures the
/// real connection from environment variables).
/// Mirrors IoBuild.Projects.Infrastructure.Persistence.AppDbContextFactory.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql(
                "Server=localhost;Database=iobuild_iam;User=root;Password=root;",
                ServerVersion.Parse("8.0.36-mysql"))
            .Options;

        return new ApplicationDbContext(options);
    }
}
