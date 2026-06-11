using IoBuild.Profiles.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Profiles.Infrastructure.Persistence.EFC.Configuration.Extensions;

public static class ModelBuilderExtensions
{
    public static void ApplyProfilesConfiguration(this ModelBuilder builder)
    {
        // Profiles Context
        
        builder.Entity<Profile>().HasKey(p => p.Id);
        builder.Entity<Profile>().Property(p => p.Id).IsRequired().ValueGeneratedOnAdd();
        builder.Entity<Profile>().Property(p => p.UserId).IsRequired();
        builder.Entity<Profile>().Property(p => p.PhotoUrl).HasMaxLength(500);
        builder.Entity<Profile>().Property(p => p.Name).IsRequired().HasMaxLength(200);
        builder.Entity<Profile>().Property(p => p.Username).IsRequired().HasMaxLength(100);
        builder.Entity<Profile>().Property(p => p.Address).HasMaxLength(500);
        builder.Entity<Profile>().Property(p => p.Age).IsRequired();
        builder.Entity<Profile>().Property(p => p.PhoneNumber).HasMaxLength(20);
        builder.Entity<Profile>().Property(p => p.SecondEmail).HasMaxLength(100).IsRequired(false).HasColumnName("second_email");
    }
}
