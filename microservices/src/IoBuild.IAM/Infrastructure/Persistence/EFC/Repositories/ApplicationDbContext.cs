using IoBuild.IAM.Domain.Model.Aggregates;
using IoBuild.IAM.Infrastructure.Persistence.EFC.Configuration.Seed;
using IoBuild.IAM.Infrastructure.Persistence.EFC.Repositories;
using IoBuild.Shared.Domain.Repositories;
using IoBuild.Shared.Infrastructure.EFC.Extensions;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users { get; set; }

    public async Task CompleteAsync()
    {
        await SaveChangesAsync();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.UseSnakeCaseNamingConvention();

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Role).IsRequired().HasMaxLength(50);
        });

        // Apply seed data AFTER entity configuration and naming conventions
        modelBuilder.ApplyIamSeedData();
    }
}
