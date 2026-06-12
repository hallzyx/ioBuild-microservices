using IoBuild.IAM.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;
using BCryptNet = BCrypt.Net.BCrypt;

namespace IoBuild.IAM.Infrastructure.Persistence.EFC.Configuration.Seed;

public static class IamSeedData
{
    public static void ApplyIamSeedData(this ModelBuilder builder)
    {
        builder.Entity<User>().HasData(
            new
            {
                Id = 1,
                Email = "builder@iobuilt.com",
                PasswordHash = BCryptNet.HashPassword("Builder123!"),
                Role = "builder"
            },
            new
            {
                Id = 2,
                Email = "owner@iobuilt.com",
                PasswordHash = BCryptNet.HashPassword("Owner123!"),
                Role = "owner"
            }
        );
    }
}
