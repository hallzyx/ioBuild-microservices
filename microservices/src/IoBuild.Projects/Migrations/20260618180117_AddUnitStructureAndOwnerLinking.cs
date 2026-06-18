using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoBuild.Projects.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitStructureAndOwnerLinking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "owner_id",
                table: "units",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "floor",
                table: "units",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "owner_email",
                table: "units",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "room_number",
                table: "units",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "registered_owner",
                columns: table => new
                {
                    email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    user_id = table.Column<int>(type: "int", nullable: false),
                    last_event_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_registered_owner", x => x.email);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "units",
                keyColumn: "id",
                keyValue: 1,
                columns: new[] { "floor", "owner_email", "room_number" },
                values: new object[] { 5, null, "01" });

            migrationBuilder.UpdateData(
                table: "units",
                keyColumn: "id",
                keyValue: 2,
                columns: new[] { "floor", "owner_email", "room_number" },
                values: new object[] { 5, null, "02" });

            migrationBuilder.UpdateData(
                table: "units",
                keyColumn: "id",
                keyValue: 3,
                columns: new[] { "floor", "owner_email", "room_number" },
                values: new object[] { 8, null, "01" });

            migrationBuilder.UpdateData(
                table: "units",
                keyColumn: "id",
                keyValue: 4,
                columns: new[] { "floor", "owner_email", "room_number" },
                values: new object[] { 12, null, "05" });

            migrationBuilder.UpdateData(
                table: "units",
                keyColumn: "id",
                keyValue: 5,
                columns: new[] { "floor", "owner_email", "room_number" },
                values: new object[] { 8, null, "01-T2" });

            migrationBuilder.CreateIndex(
                name: "IX_units_owner_email",
                table: "units",
                column: "owner_email");

            migrationBuilder.CreateIndex(
                name: "IX_units_project_id_floor_room_number",
                table: "units",
                columns: new[] { "project_id", "floor", "room_number" },
                unique: true);

            // Drop the now-redundant single-column FK index AFTER the composite
            // index exists. MySQL keeps the Unit->Project FK satisfied by the
            // composite index (project_id is its leading column), so the drop is
            // only valid once the composite index is in place.
            migrationBuilder.DropIndex(
                name: "IX_units_project_id",
                table: "units");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "registered_owner");

            migrationBuilder.DropIndex(
                name: "IX_units_owner_email",
                table: "units");

            migrationBuilder.DropIndex(
                name: "IX_units_project_id_floor_room_number",
                table: "units");

            migrationBuilder.DropColumn(
                name: "floor",
                table: "units");

            migrationBuilder.DropColumn(
                name: "owner_email",
                table: "units");

            migrationBuilder.DropColumn(
                name: "room_number",
                table: "units");

            migrationBuilder.AlterColumn<int>(
                name: "owner_id",
                table: "units",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_units_project_id",
                table: "units",
                column: "project_id");
        }
    }
}
