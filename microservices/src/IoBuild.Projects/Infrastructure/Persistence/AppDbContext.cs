using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Shared.Domain.Repositories;
using IoBuild.Shared.Infrastructure.EFC.Extensions;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Projects.Infrastructure.Persistence;

public class AppDbContext : DbContext, IUnitOfWork
{
    public DbSet<Project> Projects { get; set; }
    public DbSet<Unit> Units { get; set; }
    public DbSet<Client> Clients { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public async Task CompleteAsync()
    {
        await SaveChangesAsync();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.UseSnakeCaseNamingConvention();

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Description).HasMaxLength(2000);
            entity.Property(p => p.Location).HasMaxLength(500);
            entity.Property(p => p.Status)
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(p => p.CreatedDate);
            entity.Property(p => p.ImageUrl).HasMaxLength(1000);
        });

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.Property(u => u.UnitNumber).IsRequired().HasMaxLength(50);
            entity.HasOne<Project>()
                .WithMany()
                .HasForeignKey(u => u.ProjectId);
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.FullName).IsRequired().HasMaxLength(200);
            entity.Property(c => c.ProjectName).HasMaxLength(200);
            entity.Property(c => c.AccountStatement)
                .HasConversion<string>()
                .HasMaxLength(50);
            entity.Property(c => c.Email).HasMaxLength(200);
            entity.Property(c => c.PhoneNumber).HasMaxLength(50);
            entity.Property(c => c.Address).HasMaxLength(500);
            entity.HasOne<Project>()
                .WithMany()
                .HasForeignKey(c => c.ProjectId);
        });
    }
}
