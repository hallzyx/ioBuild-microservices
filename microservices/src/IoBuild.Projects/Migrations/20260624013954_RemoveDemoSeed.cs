using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IoBuild.Projects.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDemoSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "clients",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "clients",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "clients",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "clients",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "clients",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "clients",
                keyColumn: "id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "clients",
                keyColumn: "id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "clients",
                keyColumn: "id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "clients",
                keyColumn: "id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "clients",
                keyColumn: "id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "clients",
                keyColumn: "id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "clients",
                keyColumn: "id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "clients",
                keyColumn: "id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "clients",
                keyColumn: "id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "units",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "units",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "units",
                keyColumn: "id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "units",
                keyColumn: "id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "units",
                keyColumn: "id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "projects",
                keyColumn: "id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "projects",
                keyColumn: "id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "projects",
                keyColumn: "id",
                keyValue: 3);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "projects",
                columns: new[] { "id", "builder_id", "created_date", "description", "image_url", "location", "name", "occupied_units", "status", "total_units" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2024, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Complejo residencial de lujo con 120 departamentos en San Isidro. Cuenta con áreas verdes, piscina, gimnasio y vigilancia 24/7.", "https://images.unsplash.com/photo-1545324418-cc1a3fa10c00?w=800", "Av. Conquistadores 890, San Isidro, Lima", "Residencial Los Álamos", 95, "OnGoing", 120 },
                    { 2, 1, new DateTime(2024, 6, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Desarrollo de dos torres con vista al mar en Miraflores. 80 departamentos premium con acabados de primera calidad.", "https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?w=800", "Malecón de la Reserva 456, Miraflores, Lima", "Torres del Pacífico", 68, "OnGoing", 80 },
                    { 3, 1, new DateTime(2024, 9, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Proyecto residencial en construcción con 60 departamentos tipo loft en Surco. Entrega prevista para Q2 2025.", "https://images.unsplash.com/photo-1512917774080-9991f1c4c750?w=800", "Av. Primavera 1234, Santiago de Surco, Lima", "Condominio Las Casuarinas", 12, "OnGoing", 60 }
                });

            migrationBuilder.InsertData(
                table: "clients",
                columns: new[] { "id", "account_statement", "address", "email", "full_name", "phone_number", "project_id", "project_name" },
                values: new object[,]
                {
                    { 1, "Active", "Av. Arequipa 1450, Lince, Lima", "carlos.mendoza@email.com", "Carlos Mendoza Ruiz", "+51 998765432", 1, "Residencial Los Álamos" },
                    { 2, "Active", "Calle Los Olivos 234, San Isidro, Lima", "ana.torres@email.com", "Ana Lucía Torres", "+51 987654321", 1, "Residencial Los Álamos" },
                    { 3, "Pending", "Jr. Monterrey 567, La Molina, Lima", "roberto.vargas@email.com", "Roberto Vargas León", "+51 976543210", 1, "Residencial Los Álamos" },
                    { 4, "Active", "Av. Benavides 2890, Miraflores, Lima", "patricia.salazar@email.com", "Patricia Salazar Gómez", "+51 965432109", 1, "Residencial Los Álamos" },
                    { 5, "Suspended", "Calle San Martín 890, Barranco, Lima", "luis.rojas@email.com", "Luis Fernando Rojas", "+51 954321098", 1, "Residencial Los Álamos" },
                    { 6, "Active", "Malecón Cisneros 1234, Miraflores, Lima", "sandra.valverde@email.com", "Sandra Valverde Castro", "+51 943210987", 2, "Torres del Pacífico" },
                    { 7, "Active", "Av. Larco 789, Miraflores, Lima", "miguel.herrera@email.com", "Miguel Ángel Herrera", "+51 932109876", 2, "Torres del Pacífico" },
                    { 8, "Active", "Calle Shell 456, Miraflores, Lima", "gabriela.quispe@email.com", "Gabriela Quispe Flores", "+51 921098765", 2, "Torres del Pacífico" },
                    { 9, "Inactive", "Av. Angamos 2345, Surquillo, Lima", "fernando.diaz@email.com", "Fernando Díaz Pérez", "+51 910987654", 2, "Torres del Pacífico" },
                    { 10, "Pending", "Av. Javier Prado Este 4567, Surco, Lima", "maria.vega@email.com", "María Elena Vega", "+51 909876543", 3, "Condominio Las Casuarinas" },
                    { 11, "Pending", "Calle Las Camelias 345, San Isidro, Lima", "jorge.campos@email.com", "Jorge Luis Campos", "+51 998876543", 3, "Condominio Las Casuarinas" },
                    { 12, "Active", "Av. Primavera 890, Surco, Lima", "roxana.gutierrez@email.com", "Roxana Gutiérrez Silva", "+51 987765432", 3, "Condominio Las Casuarinas" },
                    { 13, "Pending", "Calle Los Eucaliptos 123, Surco, Lima", "alberto.sanchez@email.com", "Alberto Sánchez Torres", "+51 976654321", 3, "Condominio Las Casuarinas" },
                    { 14, "Active", "Av. Aviación 4321, San Borja, Lima", "elena.ramirez@email.com", "Elena Ramírez Meza", "+51 965543210", 3, "Condominio Las Casuarinas" }
                });

            migrationBuilder.InsertData(
                table: "units",
                columns: new[] { "id", "floor", "owner_email", "owner_id", "project_id", "room_number", "unit_number" },
                values: new object[,]
                {
                    { 1, 5, null, 2, 1, "01", "A-501" },
                    { 2, 5, null, 2, 1, "02", "A-502" },
                    { 3, 8, null, 3, 1, "01", "B-801" },
                    { 4, 12, null, 4, 2, "05", "T1-1205" },
                    { 5, 8, null, 2, 2, "01-T2", "T2-0801" }
                });
        }
    }
}
