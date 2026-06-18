using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace IoBuild.Devices.Infrastructure.Persistence.EFC.DbContext;

/// <summary>
/// Design-time factory used by the EF Core tools (<c>dotnet ef migrations</c>).
/// Uses an explicit server version so the tooling never has to connect to a live
/// database to build the model. Not used at runtime (Program.cs configures the
/// real connection from environment variables).
/// </summary>
public class DevicesDbContextFactory : IDesignTimeDbContextFactory<DevicesDbContext>
{
    public DevicesDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<DevicesDbContext>()
            .UseMySql(
                "Server=localhost;Database=iobuild_devices;User=root;Password=root;",
                ServerVersion.Parse("8.0.36-mysql"))
            .Options;

        return new DevicesDbContext(options);
    }
}
