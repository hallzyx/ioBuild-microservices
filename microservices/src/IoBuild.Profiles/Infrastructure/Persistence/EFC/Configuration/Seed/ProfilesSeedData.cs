using IoBuild.Profiles.Domain.Model.Aggregates;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Profiles.Infrastructure.Persistence.EFC.Configuration.Seed;

public static class ProfilesSeedData
{
    public static void ApplyProfilesSeedData(this ModelBuilder builder)
    {
        builder.Entity<Profile>().HasData(
            new
            {
                Id = 1,
                UserId = 1,
                PhotoUrl = "https://randomuser.me/api/portraits/men/32.jpg",
                Name = "Juan Pérez",
                Username = "juan_builder",
                Address = "Av. Javier Prado 123, San Isidro, Lima",
                Age = 35,
                PhoneNumber = "+51 987654321"
            },
            new
            {
                Id = 2,
                UserId = 2,
                PhotoUrl = "https://randomuser.me/api/portraits/women/44.jpg",
                Name = "María González",
                Username = "maria_owner",
                Address = "Calle Las Begonias 456, San Borja, Lima",
                Age = 42,
                PhoneNumber = "+51 912345678"
            }
        );
    }
}
