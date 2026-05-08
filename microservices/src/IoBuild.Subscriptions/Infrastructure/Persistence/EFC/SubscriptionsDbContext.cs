using IoBuild.Shared.Domain.Repositories;
using IoBuild.Shared.Infrastructure.EFC.Extensions;
using IoBuild.Subscriptions.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Subscriptions.Infrastructure.Persistence.EFC;

public class SubscriptionsDbContext : DbContext, IUnitOfWork
{
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<Plan> Plans { get; set; }

    public SubscriptionsDbContext(DbContextOptions<SubscriptionsDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.UseSnakeCaseNamingConvention();

        modelBuilder.Entity<Plan>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(100);
            entity.Property(p => p.Description).HasMaxLength(500);
            entity.Property(p => p.Features).HasMaxLength(2000);
            entity.Property(p => p.SupportLevel).HasMaxLength(50);
            entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
            entity.HasMany(p => p.Subscriptions)
                  .WithOne(s => s.Plan)
                  .HasForeignKey(s => s.PlanId);
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Status)
                  .HasConversion<string>()
                  .HasMaxLength(20);
            entity.HasOne(s => s.Plan)
                  .WithMany(p => p.Subscriptions)
                  .HasForeignKey(s => s.PlanId);
        });
    }

    public async Task CompleteAsync()
    {
        await SaveChangesAsync();
    }
}
