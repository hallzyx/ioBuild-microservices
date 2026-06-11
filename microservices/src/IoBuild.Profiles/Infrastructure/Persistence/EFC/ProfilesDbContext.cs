using IoBuild.Profiles.Domain.Model.Aggregates;
using IoBuild.Profiles.Infrastructure.Persistence.EFC.Configuration.Extensions;
using IoBuild.Shared.Domain.Repositories;
using IoBuild.Shared.Infrastructure.EFC.Extensions;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Profiles.Infrastructure.Persistence.EFC;

public class ProfilesDbContext(DbContextOptions<ProfilesDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Profile> Profiles { get; set; }

    public async Task CompleteAsync()
    {
        await SaveChangesAsync();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.UseSnakeCaseNamingConvention();
        modelBuilder.ApplyProfilesConfiguration();
    }
}
