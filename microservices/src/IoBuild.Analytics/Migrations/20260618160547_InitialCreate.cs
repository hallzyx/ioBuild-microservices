using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoBuild.Analytics.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "device_health_statuses",
                columns: table => new
                {
                    device_id = table.Column<int>(type: "int", nullable: false),
                    device_name = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    last_online = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "device_projections",
                columns: table => new
                {
                    device_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    owner_user_id = table.Column<int>(type: "int", nullable: false),
                    project_id = table.Column<int>(type: "int", nullable: true),
                    unit_id = table.Column<int>(type: "int", nullable: true),
                    device_type = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    last_event_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_device_projections", x => x.device_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "historical_data_points",
                columns: table => new
                {
                    timestamp = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    value = table.Column<double>(type: "double", nullable: false),
                    metric = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "project_projections",
                columns: table => new
                {
                    project_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    builder_user_id = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "varchar(160)", maxLength: 160, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    last_event_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_projections", x => x.project_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "unit_projections",
                columns: table => new
                {
                    unit_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    project_id = table.Column<int>(type: "int", nullable: false),
                    builder_user_id = table.Column<int>(type: "int", nullable: false),
                    owner_user_id = table.Column<int>(type: "int", nullable: true),
                    status = table.Column<string>(type: "varchar(32)", maxLength: 32, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    last_event_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_unit_projections", x => x.unit_id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_device_projections_owner_user_id",
                table: "device_projections",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_device_projections_project_id",
                table: "device_projections",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_projections_builder_user_id",
                table: "project_projections",
                column: "builder_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_unit_projections_builder_user_id",
                table: "unit_projections",
                column: "builder_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_unit_projections_owner_user_id",
                table: "unit_projections",
                column: "owner_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_unit_projections_project_id",
                table: "unit_projections",
                column: "project_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "device_health_statuses");

            migrationBuilder.DropTable(
                name: "device_projections");

            migrationBuilder.DropTable(
                name: "historical_data_points");

            migrationBuilder.DropTable(
                name: "project_projections");

            migrationBuilder.DropTable(
                name: "unit_projections");
        }
    }
}
