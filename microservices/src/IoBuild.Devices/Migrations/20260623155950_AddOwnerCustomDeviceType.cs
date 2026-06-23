using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoBuild.Devices.Migrations
{
    /// <inheritdoc />
    public partial class AddOwnerCustomDeviceType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "owner_custom_device_types",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    owner_user_id = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    type_code = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    display_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    attributes_json = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_owner_custom_device_types", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1000,
                column: "timestamp",
                value: new DateTime(2026, 5, 24, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1001,
                column: "timestamp",
                value: new DateTime(2026, 5, 25, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1002,
                column: "timestamp",
                value: new DateTime(2026, 5, 26, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1003,
                column: "timestamp",
                value: new DateTime(2026, 5, 27, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1004,
                column: "timestamp",
                value: new DateTime(2026, 5, 28, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1005,
                column: "timestamp",
                value: new DateTime(2026, 5, 29, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1006,
                column: "timestamp",
                value: new DateTime(2026, 5, 30, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1007,
                column: "timestamp",
                value: new DateTime(2026, 5, 31, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1008,
                column: "timestamp",
                value: new DateTime(2026, 6, 1, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1009,
                column: "timestamp",
                value: new DateTime(2026, 6, 2, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1010,
                column: "timestamp",
                value: new DateTime(2026, 6, 3, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1011,
                column: "timestamp",
                value: new DateTime(2026, 6, 4, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1012,
                column: "timestamp",
                value: new DateTime(2026, 6, 5, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1013,
                column: "timestamp",
                value: new DateTime(2026, 6, 6, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1014,
                column: "timestamp",
                value: new DateTime(2026, 6, 7, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1015,
                column: "timestamp",
                value: new DateTime(2026, 6, 8, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1016,
                column: "timestamp",
                value: new DateTime(2026, 6, 9, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1017,
                column: "timestamp",
                value: new DateTime(2026, 6, 10, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1018,
                column: "timestamp",
                value: new DateTime(2026, 6, 11, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1019,
                column: "timestamp",
                value: new DateTime(2026, 6, 12, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1020,
                column: "timestamp",
                value: new DateTime(2026, 6, 13, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1021,
                column: "timestamp",
                value: new DateTime(2026, 6, 14, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1022,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1023,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1024,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1025,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1026,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1027,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1028,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1029,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2000,
                column: "timestamp",
                value: new DateTime(2026, 5, 24, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2001,
                column: "timestamp",
                value: new DateTime(2026, 5, 25, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2002,
                column: "timestamp",
                value: new DateTime(2026, 5, 26, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2003,
                column: "timestamp",
                value: new DateTime(2026, 5, 27, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2004,
                column: "timestamp",
                value: new DateTime(2026, 5, 28, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2005,
                column: "timestamp",
                value: new DateTime(2026, 5, 29, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2006,
                column: "timestamp",
                value: new DateTime(2026, 5, 30, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2007,
                column: "timestamp",
                value: new DateTime(2026, 5, 31, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2008,
                column: "timestamp",
                value: new DateTime(2026, 6, 1, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2009,
                column: "timestamp",
                value: new DateTime(2026, 6, 2, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2010,
                column: "timestamp",
                value: new DateTime(2026, 6, 3, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2011,
                column: "timestamp",
                value: new DateTime(2026, 6, 4, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2012,
                column: "timestamp",
                value: new DateTime(2026, 6, 5, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2013,
                column: "timestamp",
                value: new DateTime(2026, 6, 6, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2014,
                column: "timestamp",
                value: new DateTime(2026, 6, 7, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2015,
                column: "timestamp",
                value: new DateTime(2026, 6, 8, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2016,
                column: "timestamp",
                value: new DateTime(2026, 6, 9, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2017,
                column: "timestamp",
                value: new DateTime(2026, 6, 10, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2018,
                column: "timestamp",
                value: new DateTime(2026, 6, 11, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2019,
                column: "timestamp",
                value: new DateTime(2026, 6, 12, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2020,
                column: "timestamp",
                value: new DateTime(2026, 6, 13, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2021,
                column: "timestamp",
                value: new DateTime(2026, 6, 14, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2022,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2023,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2024,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2025,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2026,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2027,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2028,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2029,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3000,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 15, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3001,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 16, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3002,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 17, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3003,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 18, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3004,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 19, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3005,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 20, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3006,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 21, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3007,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 22, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3008,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 23, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3009,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 0, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3010,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 1, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3011,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 2, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3012,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 3, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3013,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 4, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3014,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 5, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3015,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 6, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3016,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 7, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3017,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 8, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3018,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 9, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3019,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 10, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3020,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 11, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3021,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 12, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3022,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 13, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3023,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 14, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3024,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 15, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3025,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 16, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3026,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 17, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3027,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 18, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3028,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 19, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3029,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 20, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3030,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 21, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3031,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 22, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3032,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 23, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3033,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 0, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3034,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 1, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3035,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 2, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3036,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 3, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3037,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 4, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3038,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 5, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3039,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 6, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3040,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 7, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3041,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 8, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3042,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 9, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3043,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 10, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3044,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 11, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3045,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 12, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3046,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 13, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3047,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 14, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3048,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 15, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3049,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 16, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3050,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 17, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3051,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 18, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3052,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 19, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3053,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 20, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3054,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 21, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3055,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 22, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3056,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 23, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3057,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 0, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3058,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 1, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3059,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 2, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3060,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 3, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3061,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 4, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3062,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 5, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3063,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 6, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3064,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 7, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3065,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 8, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3066,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 9, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3067,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 10, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3068,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 11, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3069,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 12, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3070,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 13, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3071,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 14, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3072,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 15, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3073,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 16, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3074,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 17, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3075,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 18, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3076,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 19, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3077,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 20, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3078,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 21, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3079,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 22, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3080,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 23, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3081,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 0, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3082,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 1, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3083,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 2, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3084,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 3, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3085,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 4, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3086,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 5, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3087,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 6, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3088,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 7, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3089,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 8, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3090,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 9, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3091,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 10, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3092,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 11, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3093,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 12, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3094,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 13, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3095,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 14, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3096,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 15, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3097,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 16, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3098,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 17, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3099,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 18, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3100,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 19, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3101,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 20, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3102,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 21, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3103,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 22, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3104,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 23, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3105,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 0, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3106,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 1, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3107,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 2, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3108,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 3, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3109,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 4, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3110,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 5, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3111,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 6, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3112,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 7, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3113,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 8, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3114,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 9, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3115,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 10, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3116,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 11, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3117,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 12, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3118,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 13, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3119,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 14, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3120,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 15, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3121,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 16, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3122,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 17, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3123,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 18, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3124,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 19, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3125,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 20, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3126,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 21, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3127,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 22, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3128,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 23, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3129,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 0, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3130,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 1, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3131,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 2, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3132,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 3, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3133,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 4, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3134,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 5, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3135,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 6, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3136,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 7, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3137,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 8, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3138,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 9, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3139,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 10, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3140,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 11, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3141,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 12, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3142,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 13, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3143,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 14, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3144,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 15, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3145,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 16, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3146,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 17, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3147,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 18, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3148,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 19, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3149,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 20, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3150,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 21, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3151,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 22, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3152,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 23, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3153,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 0, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3154,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 1, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3155,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 2, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3156,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 3, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3157,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 4, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3158,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 5, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3159,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 6, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3160,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 7, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3161,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 8, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3162,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 9, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3163,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 10, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3164,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 11, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3165,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 12, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3166,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 13, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3167,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 14, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4000,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 15, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4001,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 16, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4002,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 17, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4003,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 18, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4004,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 19, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4005,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 20, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4006,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 21, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4007,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 22, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4008,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 23, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4009,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 0, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4010,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 1, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4011,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 2, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4012,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 3, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4013,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 4, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4014,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 5, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4015,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 6, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4016,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 7, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4017,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 8, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4018,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 9, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4019,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 10, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4020,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 11, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4021,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 12, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4022,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 13, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4023,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 14, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4024,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 15, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4025,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 16, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4026,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 17, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4027,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 18, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4028,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 19, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4029,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 20, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4030,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 21, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4031,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 22, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4032,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 23, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4033,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 0, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4034,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 1, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4035,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 2, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4036,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 3, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4037,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 4, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4038,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 5, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4039,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 6, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4040,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 7, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4041,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 8, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4042,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 9, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4043,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 10, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4044,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 11, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4045,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 12, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4046,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 13, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4047,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 14, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4048,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 15, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4049,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 16, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4050,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 17, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4051,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 18, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4052,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 19, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4053,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 20, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4054,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 21, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4055,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 22, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4056,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 23, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4057,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 0, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4058,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 1, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4059,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 2, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4060,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 3, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4061,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 4, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4062,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 5, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4063,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 6, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4064,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 7, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4065,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 8, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4066,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 9, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4067,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 10, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4068,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 11, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4069,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 12, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4070,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 13, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4071,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 14, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4072,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 15, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4073,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 16, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4074,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 17, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4075,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 18, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4076,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 19, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4077,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 20, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4078,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 21, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4079,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 22, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4080,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 23, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4081,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 0, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4082,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 1, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4083,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 2, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4084,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 3, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4085,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 4, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4086,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 5, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4087,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 6, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4088,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 7, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4089,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 8, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4090,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 9, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4091,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 10, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4092,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 11, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4093,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 12, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4094,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 13, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4095,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 14, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4096,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 15, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4097,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 16, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4098,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 17, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4099,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 18, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4100,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 19, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4101,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 20, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4102,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 21, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4103,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 22, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4104,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 23, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4105,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 0, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4106,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 1, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4107,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 2, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4108,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 3, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4109,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 4, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4110,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 5, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4111,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 6, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4112,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 7, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4113,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 8, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4114,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 9, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4115,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 10, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4116,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 11, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4117,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 12, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4118,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 13, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4119,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 14, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4120,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 15, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4121,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 16, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4122,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 17, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4123,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 18, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4124,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 19, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4125,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 20, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4126,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 21, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4127,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 22, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4128,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 23, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4129,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 0, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4130,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 1, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4131,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 2, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4132,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 3, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4133,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 4, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4134,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 5, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4135,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 6, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4136,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 7, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4137,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 8, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4138,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 9, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4139,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 10, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4140,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 11, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4141,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 12, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4142,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 13, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4143,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 14, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4144,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 15, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4145,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 16, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4146,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 17, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4147,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 18, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4148,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 19, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4149,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 20, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4150,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 21, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4151,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 22, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4152,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 23, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4153,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 0, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4154,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 1, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4155,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 2, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4156,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 3, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4157,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 4, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4158,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 5, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4159,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 6, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4160,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 7, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4161,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 8, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4162,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 9, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4163,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 10, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4164,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 11, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4165,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 12, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4166,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 13, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4167,
                column: "timestamp",
                value: new DateTime(2026, 6, 23, 14, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5000,
                column: "timestamp",
                value: new DateTime(2026, 5, 24, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5001,
                column: "timestamp",
                value: new DateTime(2026, 5, 25, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5002,
                column: "timestamp",
                value: new DateTime(2026, 5, 26, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5003,
                column: "timestamp",
                value: new DateTime(2026, 5, 27, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5004,
                column: "timestamp",
                value: new DateTime(2026, 5, 28, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5005,
                column: "timestamp",
                value: new DateTime(2026, 5, 29, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5006,
                column: "timestamp",
                value: new DateTime(2026, 5, 30, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5007,
                column: "timestamp",
                value: new DateTime(2026, 5, 31, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5008,
                column: "timestamp",
                value: new DateTime(2026, 6, 1, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5009,
                column: "timestamp",
                value: new DateTime(2026, 6, 2, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5010,
                column: "timestamp",
                value: new DateTime(2026, 6, 3, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5011,
                column: "timestamp",
                value: new DateTime(2026, 6, 4, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5012,
                column: "timestamp",
                value: new DateTime(2026, 6, 5, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5013,
                column: "timestamp",
                value: new DateTime(2026, 6, 6, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5014,
                column: "timestamp",
                value: new DateTime(2026, 6, 7, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5015,
                column: "timestamp",
                value: new DateTime(2026, 6, 8, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5016,
                column: "timestamp",
                value: new DateTime(2026, 6, 9, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5017,
                column: "timestamp",
                value: new DateTime(2026, 6, 10, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5018,
                column: "timestamp",
                value: new DateTime(2026, 6, 11, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5019,
                column: "timestamp",
                value: new DateTime(2026, 6, 12, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5020,
                column: "timestamp",
                value: new DateTime(2026, 6, 13, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5021,
                column: "timestamp",
                value: new DateTime(2026, 6, 14, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5022,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5023,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5024,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5025,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5026,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5027,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5028,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5029,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 15, 59, 49, 195, DateTimeKind.Utc).AddTicks(5915));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6000,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 15, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6001,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 15, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6002,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 15, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6003,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 15, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6004,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 15, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6005,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 15, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6006,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 15, 59, 49, 214, DateTimeKind.Utc).AddTicks(1551));

            migrationBuilder.CreateIndex(
                name: "IX_owner_custom_device_types_owner_user_id_type_code",
                table: "owner_custom_device_types",
                columns: new[] { "owner_user_id", "type_code" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "owner_custom_device_types");

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1000,
                column: "timestamp",
                value: new DateTime(2026, 5, 23, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1001,
                column: "timestamp",
                value: new DateTime(2026, 5, 24, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1002,
                column: "timestamp",
                value: new DateTime(2026, 5, 25, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1003,
                column: "timestamp",
                value: new DateTime(2026, 5, 26, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1004,
                column: "timestamp",
                value: new DateTime(2026, 5, 27, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1005,
                column: "timestamp",
                value: new DateTime(2026, 5, 28, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1006,
                column: "timestamp",
                value: new DateTime(2026, 5, 29, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1007,
                column: "timestamp",
                value: new DateTime(2026, 5, 30, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1008,
                column: "timestamp",
                value: new DateTime(2026, 5, 31, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1009,
                column: "timestamp",
                value: new DateTime(2026, 6, 1, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1010,
                column: "timestamp",
                value: new DateTime(2026, 6, 2, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1011,
                column: "timestamp",
                value: new DateTime(2026, 6, 3, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1012,
                column: "timestamp",
                value: new DateTime(2026, 6, 4, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1013,
                column: "timestamp",
                value: new DateTime(2026, 6, 5, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1014,
                column: "timestamp",
                value: new DateTime(2026, 6, 6, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1015,
                column: "timestamp",
                value: new DateTime(2026, 6, 7, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1016,
                column: "timestamp",
                value: new DateTime(2026, 6, 8, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1017,
                column: "timestamp",
                value: new DateTime(2026, 6, 9, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1018,
                column: "timestamp",
                value: new DateTime(2026, 6, 10, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1019,
                column: "timestamp",
                value: new DateTime(2026, 6, 11, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1020,
                column: "timestamp",
                value: new DateTime(2026, 6, 12, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1021,
                column: "timestamp",
                value: new DateTime(2026, 6, 13, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1022,
                column: "timestamp",
                value: new DateTime(2026, 6, 14, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1023,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1024,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1025,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1026,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1027,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1028,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1029,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2000,
                column: "timestamp",
                value: new DateTime(2026, 5, 23, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2001,
                column: "timestamp",
                value: new DateTime(2026, 5, 24, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2002,
                column: "timestamp",
                value: new DateTime(2026, 5, 25, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2003,
                column: "timestamp",
                value: new DateTime(2026, 5, 26, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2004,
                column: "timestamp",
                value: new DateTime(2026, 5, 27, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2005,
                column: "timestamp",
                value: new DateTime(2026, 5, 28, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2006,
                column: "timestamp",
                value: new DateTime(2026, 5, 29, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2007,
                column: "timestamp",
                value: new DateTime(2026, 5, 30, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2008,
                column: "timestamp",
                value: new DateTime(2026, 5, 31, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2009,
                column: "timestamp",
                value: new DateTime(2026, 6, 1, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2010,
                column: "timestamp",
                value: new DateTime(2026, 6, 2, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2011,
                column: "timestamp",
                value: new DateTime(2026, 6, 3, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2012,
                column: "timestamp",
                value: new DateTime(2026, 6, 4, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2013,
                column: "timestamp",
                value: new DateTime(2026, 6, 5, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2014,
                column: "timestamp",
                value: new DateTime(2026, 6, 6, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2015,
                column: "timestamp",
                value: new DateTime(2026, 6, 7, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2016,
                column: "timestamp",
                value: new DateTime(2026, 6, 8, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2017,
                column: "timestamp",
                value: new DateTime(2026, 6, 9, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2018,
                column: "timestamp",
                value: new DateTime(2026, 6, 10, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2019,
                column: "timestamp",
                value: new DateTime(2026, 6, 11, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2020,
                column: "timestamp",
                value: new DateTime(2026, 6, 12, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2021,
                column: "timestamp",
                value: new DateTime(2026, 6, 13, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2022,
                column: "timestamp",
                value: new DateTime(2026, 6, 14, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2023,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2024,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2025,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2026,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2027,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2028,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2029,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3000,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 23, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3001,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 0, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3002,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 1, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3003,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 2, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3004,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 3, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3005,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 4, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3006,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 5, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3007,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 6, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3008,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 7, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3009,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 8, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3010,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 9, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3011,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 10, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3012,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 11, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3013,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 12, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3014,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 13, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3015,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 14, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3016,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 15, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3017,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 16, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3018,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 17, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3019,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 18, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3020,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 19, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3021,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 20, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3022,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 21, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3023,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 22, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3024,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 23, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3025,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 0, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3026,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 1, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3027,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 2, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3028,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 3, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3029,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 4, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3030,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 5, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3031,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 6, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3032,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 7, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3033,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 8, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3034,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 9, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3035,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 10, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3036,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 11, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3037,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 12, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3038,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 13, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3039,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 14, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3040,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 15, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3041,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 16, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3042,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 17, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3043,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 18, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3044,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 19, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3045,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 20, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3046,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 21, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3047,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 22, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3048,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 23, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3049,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 0, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3050,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 1, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3051,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 2, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3052,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 3, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3053,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 4, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3054,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 5, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3055,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 6, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3056,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 7, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3057,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 8, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3058,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 9, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3059,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 10, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3060,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 11, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3061,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 12, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3062,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 13, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3063,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 14, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3064,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 15, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3065,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 16, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3066,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 17, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3067,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 18, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3068,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 19, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3069,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 20, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3070,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 21, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3071,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 22, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3072,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 23, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3073,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 0, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3074,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 1, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3075,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 2, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3076,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 3, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3077,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 4, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3078,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 5, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3079,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 6, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3080,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 7, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3081,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 8, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3082,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 9, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3083,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 10, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3084,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 11, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3085,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 12, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3086,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 13, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3087,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 14, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3088,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 15, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3089,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 16, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3090,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 17, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3091,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 18, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3092,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 19, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3093,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 20, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3094,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 21, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3095,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 22, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3096,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 23, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3097,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 0, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3098,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 1, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3099,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 2, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3100,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 3, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3101,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 4, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3102,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 5, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3103,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 6, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3104,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 7, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3105,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 8, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3106,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 9, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3107,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 10, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3108,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 11, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3109,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 12, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3110,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 13, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3111,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 14, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3112,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 15, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3113,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 16, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3114,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 17, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3115,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 18, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3116,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 19, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3117,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 20, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3118,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 21, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3119,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 22, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3120,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 23, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3121,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 0, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3122,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 1, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3123,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 2, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3124,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 3, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3125,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 4, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3126,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 5, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3127,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 6, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3128,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 7, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3129,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 8, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3130,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 9, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3131,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 10, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3132,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 11, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3133,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 12, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3134,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 13, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3135,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 14, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3136,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 15, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3137,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 16, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3138,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 17, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3139,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 18, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3140,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 19, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3141,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 20, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3142,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 21, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3143,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 22, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3144,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 23, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3145,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 0, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3146,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 1, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3147,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 2, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3148,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 3, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3149,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 4, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3150,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 5, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3151,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 6, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3152,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 7, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3153,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 8, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3154,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 9, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3155,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 10, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3156,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 11, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3157,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 12, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3158,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 13, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3159,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 14, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3160,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 15, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3161,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 16, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3162,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 17, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3163,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 18, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3164,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 19, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3165,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 20, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3166,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 21, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3167,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 22, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4000,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 23, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4001,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 0, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4002,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 1, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4003,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 2, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4004,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 3, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4005,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 4, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4006,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 5, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4007,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 6, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4008,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 7, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4009,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 8, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4010,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 9, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4011,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 10, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4012,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 11, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4013,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 12, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4014,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 13, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4015,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 14, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4016,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 15, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4017,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 16, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4018,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 17, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4019,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 18, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4020,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 19, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4021,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 20, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4022,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 21, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4023,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 22, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4024,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 23, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4025,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 0, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4026,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 1, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4027,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 2, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4028,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 3, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4029,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 4, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4030,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 5, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4031,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 6, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4032,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 7, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4033,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 8, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4034,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 9, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4035,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 10, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4036,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 11, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4037,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 12, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4038,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 13, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4039,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 14, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4040,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 15, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4041,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 16, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4042,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 17, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4043,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 18, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4044,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 19, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4045,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 20, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4046,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 21, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4047,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 22, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4048,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 23, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4049,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 0, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4050,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 1, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4051,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 2, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4052,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 3, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4053,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 4, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4054,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 5, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4055,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 6, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4056,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 7, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4057,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 8, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4058,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 9, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4059,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 10, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4060,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 11, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4061,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 12, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4062,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 13, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4063,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 14, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4064,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 15, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4065,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 16, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4066,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 17, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4067,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 18, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4068,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 19, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4069,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 20, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4070,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 21, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4071,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 22, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4072,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 23, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4073,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 0, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4074,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 1, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4075,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 2, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4076,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 3, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4077,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 4, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4078,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 5, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4079,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 6, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4080,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 7, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4081,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 8, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4082,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 9, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4083,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 10, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4084,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 11, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4085,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 12, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4086,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 13, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4087,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 14, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4088,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 15, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4089,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 16, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4090,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 17, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4091,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 18, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4092,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 19, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4093,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 20, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4094,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 21, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4095,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 22, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4096,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 23, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4097,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 0, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4098,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 1, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4099,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 2, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4100,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 3, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4101,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 4, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4102,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 5, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4103,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 6, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4104,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 7, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4105,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 8, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4106,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 9, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4107,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 10, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4108,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 11, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4109,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 12, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4110,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 13, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4111,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 14, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4112,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 15, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4113,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 16, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4114,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 17, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4115,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 18, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4116,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 19, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4117,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 20, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4118,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 21, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4119,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 22, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4120,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 23, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4121,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 0, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4122,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 1, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4123,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 2, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4124,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 3, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4125,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 4, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4126,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 5, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4127,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 6, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4128,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 7, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4129,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 8, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4130,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 9, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4131,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 10, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4132,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 11, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4133,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 12, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4134,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 13, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4135,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 14, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4136,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 15, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4137,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 16, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4138,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 17, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4139,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 18, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4140,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 19, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4141,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 20, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4142,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 21, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4143,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 22, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4144,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 23, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4145,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 0, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4146,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 1, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4147,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 2, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4148,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 3, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4149,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 4, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4150,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 5, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4151,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 6, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4152,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 7, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4153,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 8, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4154,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 9, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4155,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 10, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4156,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 11, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4157,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 12, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4158,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 13, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4159,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 14, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4160,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 15, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4161,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 16, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4162,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 17, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4163,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 18, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4164,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 19, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4165,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 20, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4166,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 21, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4167,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 22, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5000,
                column: "timestamp",
                value: new DateTime(2026, 5, 23, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5001,
                column: "timestamp",
                value: new DateTime(2026, 5, 24, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5002,
                column: "timestamp",
                value: new DateTime(2026, 5, 25, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5003,
                column: "timestamp",
                value: new DateTime(2026, 5, 26, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5004,
                column: "timestamp",
                value: new DateTime(2026, 5, 27, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5005,
                column: "timestamp",
                value: new DateTime(2026, 5, 28, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5006,
                column: "timestamp",
                value: new DateTime(2026, 5, 29, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5007,
                column: "timestamp",
                value: new DateTime(2026, 5, 30, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5008,
                column: "timestamp",
                value: new DateTime(2026, 5, 31, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5009,
                column: "timestamp",
                value: new DateTime(2026, 6, 1, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5010,
                column: "timestamp",
                value: new DateTime(2026, 6, 2, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5011,
                column: "timestamp",
                value: new DateTime(2026, 6, 3, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5012,
                column: "timestamp",
                value: new DateTime(2026, 6, 4, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5013,
                column: "timestamp",
                value: new DateTime(2026, 6, 5, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5014,
                column: "timestamp",
                value: new DateTime(2026, 6, 6, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5015,
                column: "timestamp",
                value: new DateTime(2026, 6, 7, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5016,
                column: "timestamp",
                value: new DateTime(2026, 6, 8, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5017,
                column: "timestamp",
                value: new DateTime(2026, 6, 9, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5018,
                column: "timestamp",
                value: new DateTime(2026, 6, 10, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5019,
                column: "timestamp",
                value: new DateTime(2026, 6, 11, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5020,
                column: "timestamp",
                value: new DateTime(2026, 6, 12, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5021,
                column: "timestamp",
                value: new DateTime(2026, 6, 13, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5022,
                column: "timestamp",
                value: new DateTime(2026, 6, 14, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5023,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5024,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5025,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5026,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5027,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5028,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5029,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 23, 46, 37, 914, DateTimeKind.Utc).AddTicks(38));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6000,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 23, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6001,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 23, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6002,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 23, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6003,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 23, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6004,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 23, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6005,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 23, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6006,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 23, 46, 37, 921, DateTimeKind.Utc).AddTicks(9521));
        }
    }
}
