using IoBuild.IAM.Domain.Model.Aggregates;
using IoBuild.IAM.Domain.Model.Entities;
using IoBuild.IAM.Infrastructure.Persistence.EFC.Configuration.Seed;
using IoBuild.IAM.Infrastructure.Persistence.EFC.Repositories;
using IoBuild.Shared.Domain.Repositories;
using IoBuild.Shared.Infrastructure.EFC.Extensions;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// IAM outbox table — polled by OutboxWorker, emits UserRegisteredEvent (ADR-8b, §2.2).
    /// </summary>
    public DbSet<OutboxMessage> OutboxMessages { get; set; }

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

        // ── Outbox entity config (mirrors Projects/Devices — §2.2) ──
        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.HasKey(m => m.Id);
            entity.Property(m => m.EventType).IsRequired().HasMaxLength(100);
            entity.Property(m => m.Payload).IsRequired();  // longtext in MySQL
            entity.Property(m => m.Status).IsRequired().HasMaxLength(20).HasDefaultValue("Pending");
            entity.Property(m => m.EventId);
            entity.Property(m => m.CreatedAt);
            entity.HasIndex(m => new { m.Status, m.CreatedAt });
        });

        // Apply seed data AFTER entity configuration and naming conventions
        modelBuilder.ApplyIamSeedData();
    }
}
