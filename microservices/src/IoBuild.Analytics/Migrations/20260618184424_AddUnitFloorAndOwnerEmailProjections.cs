using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoBuild.Analytics.Migrations
{
    /// <inheritdoc />
    public partial class AddUnitFloorAndOwnerEmailProjections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "floor",
                table: "unit_projections",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "owner_email",
                table: "unit_projections",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "room_number",
                table: "unit_projections",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "floor_number",
                table: "device_projections",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "floor",
                table: "unit_projections");

            migrationBuilder.DropColumn(
                name: "owner_email",
                table: "unit_projections");

            migrationBuilder.DropColumn(
                name: "room_number",
                table: "unit_projections");

            migrationBuilder.DropColumn(
                name: "floor_number",
                table: "device_projections");
        }
    }
}
