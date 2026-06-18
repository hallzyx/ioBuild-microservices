using IoBuild.Projects.Domain.Model.Aggregates;
using IoBuild.Projects.Domain.Model.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace IoBuild.Projects.Infrastructure.Persistence.EFC.Configuration.Seed;

public static class ProjectsSeedData
{
    public static void ApplyProjectsSeedData(this ModelBuilder builder)
    {
        // ==================== SEED PROJECTS ====================
        builder.Entity<Project>().HasData(
            new
            {
                Id = 1,
                Name = "Residencial Los Álamos",
                Description = "Complejo residencial de lujo con 120 departamentos en San Isidro. Cuenta con áreas verdes, piscina, gimnasio y vigilancia 24/7.",
                Location = "Av. Conquistadores 890, San Isidro, Lima",
                TotalUnits = 120,
                OccupiedUnits = 95,
                Status = EProjectStatus.OnGoing,
                BuilderId = 1,
                CreatedDate = new DateTime(2024, 3, 15),
                ImageUrl = "https://images.unsplash.com/photo-1545324418-cc1a3fa10c00?w=800"
            },
            new
            {
                Id = 2,
                Name = "Torres del Pacífico",
                Description = "Desarrollo de dos torres con vista al mar en Miraflores. 80 departamentos premium con acabados de primera calidad.",
                Location = "Malecón de la Reserva 456, Miraflores, Lima",
                TotalUnits = 80,
                OccupiedUnits = 68,
                Status = EProjectStatus.OnGoing,
                BuilderId = 1,
                CreatedDate = new DateTime(2024, 6, 20),
                ImageUrl = "https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?w=800"
            },
            new
            {
                Id = 3,
                Name = "Condominio Las Casuarinas",
                Description = "Proyecto residencial en construcción con 60 departamentos tipo loft en Surco. Entrega prevista para Q2 2025.",
                Location = "Av. Primavera 1234, Santiago de Surco, Lima",
                TotalUnits = 60,
                OccupiedUnits = 12,
                Status = EProjectStatus.OnGoing,
                BuilderId = 1,
                CreatedDate = new DateTime(2024, 9, 10),
                ImageUrl = "https://images.unsplash.com/photo-1512917774080-9991f1c4c750?w=800"
            }
        );

        // ==================== SEED UNITS ====================
        // PR 3 reconciliation (§1.6 / design.md):
        //  - Floor and RoomNumber are now required columns; all 5 rows get explicit values.
        //  - OwnerId remains as int? (keep existing values — they point at seeded IAM users).
        //  - OwnerEmail is null (pre-linking data; no IAM identity email known at seed time).
        //  - UnitNumber labels are grandfathered (ADR-F: stored label ≠ ComposeUnitNumber).
        builder.Entity<Unit>().HasData(
            new
            {
                Id = 1,
                ProjectId = 1,
                UnitNumber = "A-501",
                Floor = 5,
                RoomNumber = "01",
                OwnerEmail = (string?)null,
                OwnerId = (int?)2
            },
            new
            {
                Id = 2,
                ProjectId = 1,
                UnitNumber = "A-502",
                Floor = 5,
                RoomNumber = "02",
                OwnerEmail = (string?)null,
                OwnerId = (int?)2
            },
            new
            {
                Id = 3,
                ProjectId = 1,
                UnitNumber = "B-801",
                Floor = 8,
                RoomNumber = "01",
                OwnerEmail = (string?)null,
                OwnerId = (int?)3
            },
            new
            {
                Id = 4,
                ProjectId = 2,
                UnitNumber = "T1-1205",
                Floor = 12,
                RoomNumber = "05",
                OwnerEmail = (string?)null,
                OwnerId = (int?)4
            },
            new
            {
                Id = 5,
                ProjectId = 2,
                UnitNumber = "T2-0801",
                Floor = 8,
                RoomNumber = "01-T2",
                OwnerEmail = (string?)null,
                OwnerId = (int?)2
            }
        );

        // ==================== SEED CLIENTS ====================
        builder.Entity<Client>().HasData(
            // Clients for Project 1 (Residencial Los Álamos)
            new
            {
                Id = 1,
                FullName = "Carlos Mendoza Ruiz",
                ProjectId = 1,
                ProjectName = "Residencial Los Álamos",
                AccountStatement = EAccountStatement.Active,
                Email = "carlos.mendoza@email.com",
                PhoneNumber = "+51 998765432",
                Address = "Av. Arequipa 1450, Lince, Lima"
            },
            new
            {
                Id = 2,
                FullName = "Ana Lucía Torres",
                ProjectId = 1,
                ProjectName = "Residencial Los Álamos",
                AccountStatement = EAccountStatement.Active,
                Email = "ana.torres@email.com",
                PhoneNumber = "+51 987654321",
                Address = "Calle Los Olivos 234, San Isidro, Lima"
            },
            new
            {
                Id = 3,
                FullName = "Roberto Vargas León",
                ProjectId = 1,
                ProjectName = "Residencial Los Álamos",
                AccountStatement = EAccountStatement.Pending,
                Email = "roberto.vargas@email.com",
                PhoneNumber = "+51 976543210",
                Address = "Jr. Monterrey 567, La Molina, Lima"
            },
            new
            {
                Id = 4,
                FullName = "Patricia Salazar Gómez",
                ProjectId = 1,
                ProjectName = "Residencial Los Álamos",
                AccountStatement = EAccountStatement.Active,
                Email = "patricia.salazar@email.com",
                PhoneNumber = "+51 965432109",
                Address = "Av. Benavides 2890, Miraflores, Lima"
            },
            new
            {
                Id = 5,
                FullName = "Luis Fernando Rojas",
                ProjectId = 1,
                ProjectName = "Residencial Los Álamos",
                AccountStatement = EAccountStatement.Suspended,
                Email = "luis.rojas@email.com",
                PhoneNumber = "+51 954321098",
                Address = "Calle San Martín 890, Barranco, Lima"
            },
            // Clients for Project 2 (Torres del Pacífico)
            new
            {
                Id = 6,
                FullName = "Sandra Valverde Castro",
                ProjectId = 2,
                ProjectName = "Torres del Pacífico",
                AccountStatement = EAccountStatement.Active,
                Email = "sandra.valverde@email.com",
                PhoneNumber = "+51 943210987",
                Address = "Malecón Cisneros 1234, Miraflores, Lima"
            },
            new
            {
                Id = 7,
                FullName = "Miguel Ángel Herrera",
                ProjectId = 2,
                ProjectName = "Torres del Pacífico",
                AccountStatement = EAccountStatement.Active,
                Email = "miguel.herrera@email.com",
                PhoneNumber = "+51 932109876",
                Address = "Av. Larco 789, Miraflores, Lima"
            },
            new
            {
                Id = 8,
                FullName = "Gabriela Quispe Flores",
                ProjectId = 2,
                ProjectName = "Torres del Pacífico",
                AccountStatement = EAccountStatement.Active,
                Email = "gabriela.quispe@email.com",
                PhoneNumber = "+51 921098765",
                Address = "Calle Shell 456, Miraflores, Lima"
            },
            new
            {
                Id = 9,
                FullName = "Fernando Díaz Pérez",
                ProjectId = 2,
                ProjectName = "Torres del Pacífico",
                AccountStatement = EAccountStatement.Inactive,
                Email = "fernando.diaz@email.com",
                PhoneNumber = "+51 910987654",
                Address = "Av. Angamos 2345, Surquillo, Lima"
            },
            // Clients for Project 3 (Condominio Las Casuarinas)
            new
            {
                Id = 10,
                FullName = "María Elena Vega",
                ProjectId = 3,
                ProjectName = "Condominio Las Casuarinas",
                AccountStatement = EAccountStatement.Pending,
                Email = "maria.vega@email.com",
                PhoneNumber = "+51 909876543",
                Address = "Av. Javier Prado Este 4567, Surco, Lima"
            },
            new
            {
                Id = 11,
                FullName = "Jorge Luis Campos",
                ProjectId = 3,
                ProjectName = "Condominio Las Casuarinas",
                AccountStatement = EAccountStatement.Pending,
                Email = "jorge.campos@email.com",
                PhoneNumber = "+51 998876543",
                Address = "Calle Las Camelias 345, San Isidro, Lima"
            },
            new
            {
                Id = 12,
                FullName = "Roxana Gutiérrez Silva",
                ProjectId = 3,
                ProjectName = "Condominio Las Casuarinas",
                AccountStatement = EAccountStatement.Active,
                Email = "roxana.gutierrez@email.com",
                PhoneNumber = "+51 987765432",
                Address = "Av. Primavera 890, Surco, Lima"
            },
            new
            {
                Id = 13,
                FullName = "Alberto Sánchez Torres",
                ProjectId = 3,
                ProjectName = "Condominio Las Casuarinas",
                AccountStatement = EAccountStatement.Pending,
                Email = "alberto.sanchez@email.com",
                PhoneNumber = "+51 976654321",
                Address = "Calle Los Eucaliptos 123, Surco, Lima"
            },
            new
            {
                Id = 14,
                FullName = "Elena Ramírez Meza",
                ProjectId = 3,
                ProjectName = "Condominio Las Casuarinas",
                AccountStatement = EAccountStatement.Active,
                Email = "elena.ramirez@email.com",
                PhoneNumber = "+51 965543210",
                Address = "Av. Aviación 4321, San Borja, Lima"
            }
        );
    }
}
