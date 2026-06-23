using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoBuild.Analytics.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceProjectionDeviceName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "device_name",
                table: "device_projections",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "type",
                table: "device_health_statuses",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "device_name",
                table: "device_projections");

            migrationBuilder.DropColumn(
                name: "type",
                table: "device_health_statuses");
        }
    }
}
