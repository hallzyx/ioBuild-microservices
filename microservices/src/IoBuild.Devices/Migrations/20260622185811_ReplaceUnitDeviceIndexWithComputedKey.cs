using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IoBuild.Devices.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceUnitDeviceIndexWithComputedKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_devices_project_id_floor_number_type",
                table: "devices");

            migrationBuilder.DropIndex(
                name: "IX_devices_project_id_unit_id_type",
                table: "devices");

            migrationBuilder.AddColumn<int>(
                name: "unit_key",
                table: "devices",
                type: "int",
                nullable: false,
                computedColumnSql: "COALESCE(unit_id, 0)",
                stored: true);

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1000,
                column: "timestamp",
                value: new DateTime(2026, 5, 23, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1001,
                column: "timestamp",
                value: new DateTime(2026, 5, 24, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1002,
                column: "timestamp",
                value: new DateTime(2026, 5, 25, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1003,
                column: "timestamp",
                value: new DateTime(2026, 5, 26, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1004,
                column: "timestamp",
                value: new DateTime(2026, 5, 27, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1005,
                column: "timestamp",
                value: new DateTime(2026, 5, 28, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1006,
                column: "timestamp",
                value: new DateTime(2026, 5, 29, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1007,
                column: "timestamp",
                value: new DateTime(2026, 5, 30, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1008,
                column: "timestamp",
                value: new DateTime(2026, 5, 31, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1009,
                column: "timestamp",
                value: new DateTime(2026, 6, 1, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1010,
                column: "timestamp",
                value: new DateTime(2026, 6, 2, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1011,
                column: "timestamp",
                value: new DateTime(2026, 6, 3, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1012,
                column: "timestamp",
                value: new DateTime(2026, 6, 4, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1013,
                column: "timestamp",
                value: new DateTime(2026, 6, 5, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1014,
                column: "timestamp",
                value: new DateTime(2026, 6, 6, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1015,
                column: "timestamp",
                value: new DateTime(2026, 6, 7, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1016,
                column: "timestamp",
                value: new DateTime(2026, 6, 8, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1017,
                column: "timestamp",
                value: new DateTime(2026, 6, 9, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1018,
                column: "timestamp",
                value: new DateTime(2026, 6, 10, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1019,
                column: "timestamp",
                value: new DateTime(2026, 6, 11, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1020,
                column: "timestamp",
                value: new DateTime(2026, 6, 12, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1021,
                column: "timestamp",
                value: new DateTime(2026, 6, 13, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1022,
                column: "timestamp",
                value: new DateTime(2026, 6, 14, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1023,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1024,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1025,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1026,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1027,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1028,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1029,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2000,
                column: "timestamp",
                value: new DateTime(2026, 5, 23, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2001,
                column: "timestamp",
                value: new DateTime(2026, 5, 24, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2002,
                column: "timestamp",
                value: new DateTime(2026, 5, 25, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2003,
                column: "timestamp",
                value: new DateTime(2026, 5, 26, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2004,
                column: "timestamp",
                value: new DateTime(2026, 5, 27, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2005,
                column: "timestamp",
                value: new DateTime(2026, 5, 28, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2006,
                column: "timestamp",
                value: new DateTime(2026, 5, 29, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2007,
                column: "timestamp",
                value: new DateTime(2026, 5, 30, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2008,
                column: "timestamp",
                value: new DateTime(2026, 5, 31, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2009,
                column: "timestamp",
                value: new DateTime(2026, 6, 1, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2010,
                column: "timestamp",
                value: new DateTime(2026, 6, 2, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2011,
                column: "timestamp",
                value: new DateTime(2026, 6, 3, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2012,
                column: "timestamp",
                value: new DateTime(2026, 6, 4, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2013,
                column: "timestamp",
                value: new DateTime(2026, 6, 5, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2014,
                column: "timestamp",
                value: new DateTime(2026, 6, 6, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2015,
                column: "timestamp",
                value: new DateTime(2026, 6, 7, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2016,
                column: "timestamp",
                value: new DateTime(2026, 6, 8, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2017,
                column: "timestamp",
                value: new DateTime(2026, 6, 9, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2018,
                column: "timestamp",
                value: new DateTime(2026, 6, 10, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2019,
                column: "timestamp",
                value: new DateTime(2026, 6, 11, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2020,
                column: "timestamp",
                value: new DateTime(2026, 6, 12, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2021,
                column: "timestamp",
                value: new DateTime(2026, 6, 13, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2022,
                column: "timestamp",
                value: new DateTime(2026, 6, 14, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2023,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2024,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2025,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2026,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2027,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2028,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2029,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3000,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 18, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3001,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 19, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3002,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 20, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3003,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 21, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3004,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 22, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3005,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 23, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3006,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 0, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3007,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 1, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3008,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 2, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3009,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 3, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3010,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 4, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3011,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 5, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3012,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 6, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3013,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 7, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3014,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 8, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3015,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 9, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3016,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 10, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3017,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 11, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3018,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 12, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3019,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 13, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3020,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 14, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3021,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 15, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3022,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 16, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3023,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 17, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3024,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 18, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3025,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 19, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3026,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 20, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3027,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 21, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3028,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 22, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3029,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 23, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3030,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 0, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3031,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 1, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3032,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 2, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3033,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 3, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3034,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 4, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3035,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 5, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3036,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 6, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3037,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 7, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3038,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 8, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3039,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 9, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3040,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 10, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3041,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 11, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3042,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 12, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3043,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 13, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3044,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 14, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3045,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 15, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3046,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 16, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3047,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 17, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3048,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 18, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3049,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 19, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3050,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 20, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3051,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 21, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3052,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 22, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3053,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 23, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3054,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 0, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3055,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 1, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3056,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 2, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3057,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 3, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3058,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 4, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3059,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 5, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3060,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 6, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3061,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 7, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3062,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 8, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3063,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 9, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3064,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 10, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3065,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 11, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3066,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 12, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3067,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 13, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3068,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 14, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3069,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 15, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3070,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 16, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3071,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 17, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3072,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 18, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3073,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 19, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3074,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 20, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3075,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 21, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3076,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 22, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3077,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 23, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3078,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 0, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3079,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 1, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3080,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 2, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3081,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 3, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3082,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 4, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3083,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 5, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3084,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 6, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3085,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 7, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3086,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 8, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3087,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 9, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3088,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 10, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3089,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 11, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3090,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 12, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3091,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 13, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3092,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 14, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3093,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 15, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3094,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 16, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3095,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 17, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3096,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 18, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3097,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 19, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3098,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 20, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3099,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 21, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3100,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 22, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3101,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 23, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3102,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 0, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3103,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 1, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3104,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 2, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3105,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 3, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3106,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 4, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3107,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 5, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3108,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 6, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3109,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 7, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3110,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 8, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3111,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 9, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3112,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 10, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3113,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 11, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3114,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 12, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3115,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 13, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3116,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 14, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3117,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 15, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3118,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 16, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3119,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 17, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3120,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 18, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3121,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 19, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3122,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 20, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3123,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 21, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3124,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 22, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3125,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 23, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3126,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 0, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3127,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 1, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3128,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 2, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3129,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 3, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3130,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 4, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3131,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 5, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3132,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 6, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3133,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 7, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3134,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 8, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3135,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 9, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3136,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 10, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3137,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 11, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3138,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 12, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3139,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 13, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3140,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 14, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3141,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 15, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3142,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 16, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3143,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 17, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3144,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 18, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3145,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 19, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3146,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 20, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3147,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 21, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3148,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 22, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3149,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 23, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3150,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 0, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3151,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 1, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3152,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 2, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3153,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 3, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3154,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 4, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3155,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 5, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3156,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 6, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3157,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 7, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3158,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 8, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3159,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 9, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3160,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 10, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3161,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 11, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3162,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 12, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3163,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 13, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3164,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 14, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3165,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 15, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3166,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 16, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3167,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 17, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4000,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 18, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4001,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 19, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4002,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 20, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4003,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 21, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4004,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 22, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4005,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 23, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4006,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 0, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4007,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 1, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4008,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 2, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4009,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 3, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4010,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 4, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4011,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 5, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4012,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 6, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4013,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 7, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4014,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 8, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4015,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 9, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4016,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 10, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4017,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 11, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4018,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 12, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4019,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 13, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4020,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 14, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4021,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 15, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4022,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 16, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4023,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 17, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4024,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 18, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4025,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 19, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4026,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 20, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4027,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 21, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4028,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 22, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4029,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 23, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4030,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 0, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4031,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 1, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4032,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 2, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4033,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 3, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4034,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 4, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4035,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 5, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4036,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 6, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4037,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 7, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4038,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 8, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4039,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 9, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4040,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 10, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4041,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 11, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4042,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 12, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4043,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 13, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4044,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 14, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4045,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 15, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4046,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 16, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4047,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 17, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4048,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 18, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4049,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 19, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4050,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 20, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4051,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 21, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4052,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 22, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4053,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 23, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4054,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 0, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4055,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 1, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4056,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 2, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4057,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 3, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4058,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 4, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4059,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 5, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4060,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 6, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4061,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 7, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4062,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 8, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4063,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 9, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4064,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 10, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4065,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 11, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4066,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 12, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4067,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 13, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4068,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 14, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4069,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 15, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4070,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 16, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4071,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 17, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4072,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 18, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4073,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 19, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4074,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 20, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4075,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 21, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4076,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 22, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4077,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 23, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4078,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 0, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4079,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 1, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4080,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 2, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4081,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 3, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4082,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 4, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4083,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 5, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4084,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 6, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4085,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 7, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4086,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 8, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4087,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 9, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4088,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 10, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4089,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 11, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4090,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 12, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4091,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 13, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4092,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 14, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4093,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 15, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4094,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 16, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4095,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 17, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4096,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 18, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4097,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 19, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4098,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 20, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4099,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 21, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4100,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 22, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4101,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 23, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4102,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 0, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4103,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 1, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4104,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 2, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4105,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 3, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4106,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 4, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4107,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 5, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4108,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 6, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4109,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 7, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4110,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 8, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4111,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 9, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4112,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 10, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4113,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 11, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4114,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 12, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4115,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 13, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4116,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 14, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4117,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 15, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4118,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 16, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4119,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 17, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4120,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 18, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4121,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 19, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4122,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 20, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4123,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 21, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4124,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 22, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4125,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 23, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4126,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 0, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4127,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 1, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4128,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 2, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4129,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 3, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4130,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 4, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4131,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 5, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4132,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 6, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4133,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 7, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4134,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 8, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4135,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 9, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4136,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 10, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4137,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 11, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4138,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 12, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4139,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 13, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4140,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 14, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4141,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 15, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4142,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 16, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4143,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 17, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4144,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 18, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4145,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 19, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4146,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 20, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4147,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 21, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4148,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 22, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4149,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 23, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4150,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 0, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4151,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 1, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4152,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 2, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4153,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 3, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4154,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 4, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4155,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 5, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4156,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 6, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4157,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 7, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4158,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 8, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4159,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 9, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4160,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 10, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4161,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 11, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4162,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 12, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4163,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 13, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4164,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 14, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4165,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 15, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4166,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 16, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4167,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 17, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5000,
                column: "timestamp",
                value: new DateTime(2026, 5, 23, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5001,
                column: "timestamp",
                value: new DateTime(2026, 5, 24, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5002,
                column: "timestamp",
                value: new DateTime(2026, 5, 25, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5003,
                column: "timestamp",
                value: new DateTime(2026, 5, 26, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5004,
                column: "timestamp",
                value: new DateTime(2026, 5, 27, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5005,
                column: "timestamp",
                value: new DateTime(2026, 5, 28, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5006,
                column: "timestamp",
                value: new DateTime(2026, 5, 29, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5007,
                column: "timestamp",
                value: new DateTime(2026, 5, 30, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5008,
                column: "timestamp",
                value: new DateTime(2026, 5, 31, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5009,
                column: "timestamp",
                value: new DateTime(2026, 6, 1, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5010,
                column: "timestamp",
                value: new DateTime(2026, 6, 2, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5011,
                column: "timestamp",
                value: new DateTime(2026, 6, 3, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5012,
                column: "timestamp",
                value: new DateTime(2026, 6, 4, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5013,
                column: "timestamp",
                value: new DateTime(2026, 6, 5, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5014,
                column: "timestamp",
                value: new DateTime(2026, 6, 6, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5015,
                column: "timestamp",
                value: new DateTime(2026, 6, 7, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5016,
                column: "timestamp",
                value: new DateTime(2026, 6, 8, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5017,
                column: "timestamp",
                value: new DateTime(2026, 6, 9, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5018,
                column: "timestamp",
                value: new DateTime(2026, 6, 10, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5019,
                column: "timestamp",
                value: new DateTime(2026, 6, 11, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5020,
                column: "timestamp",
                value: new DateTime(2026, 6, 12, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5021,
                column: "timestamp",
                value: new DateTime(2026, 6, 13, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5022,
                column: "timestamp",
                value: new DateTime(2026, 6, 14, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5023,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5024,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5025,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5026,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5027,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5028,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5029,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 18, 58, 10, 484, DateTimeKind.Utc).AddTicks(692));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6000,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 18, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6001,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 18, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6002,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 18, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6003,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 18, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6004,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 18, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6005,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 18, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6006,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 18, 58, 10, 491, DateTimeKind.Utc).AddTicks(9531));

            migrationBuilder.CreateIndex(
                name: "IX_devices_project_id_floor_number_unit_key_type",
                table: "devices",
                columns: new[] { "project_id", "floor_number", "unit_key", "type" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_devices_project_id_floor_number_unit_key_type",
                table: "devices");

            migrationBuilder.DropColumn(
                name: "unit_key",
                table: "devices");

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1000,
                column: "timestamp",
                value: new DateTime(2026, 5, 23, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1001,
                column: "timestamp",
                value: new DateTime(2026, 5, 24, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1002,
                column: "timestamp",
                value: new DateTime(2026, 5, 25, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1003,
                column: "timestamp",
                value: new DateTime(2026, 5, 26, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1004,
                column: "timestamp",
                value: new DateTime(2026, 5, 27, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1005,
                column: "timestamp",
                value: new DateTime(2026, 5, 28, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1006,
                column: "timestamp",
                value: new DateTime(2026, 5, 29, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1007,
                column: "timestamp",
                value: new DateTime(2026, 5, 30, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1008,
                column: "timestamp",
                value: new DateTime(2026, 5, 31, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1009,
                column: "timestamp",
                value: new DateTime(2026, 6, 1, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1010,
                column: "timestamp",
                value: new DateTime(2026, 6, 2, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1011,
                column: "timestamp",
                value: new DateTime(2026, 6, 3, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1012,
                column: "timestamp",
                value: new DateTime(2026, 6, 4, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1013,
                column: "timestamp",
                value: new DateTime(2026, 6, 5, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1014,
                column: "timestamp",
                value: new DateTime(2026, 6, 6, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1015,
                column: "timestamp",
                value: new DateTime(2026, 6, 7, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1016,
                column: "timestamp",
                value: new DateTime(2026, 6, 8, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1017,
                column: "timestamp",
                value: new DateTime(2026, 6, 9, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1018,
                column: "timestamp",
                value: new DateTime(2026, 6, 10, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1019,
                column: "timestamp",
                value: new DateTime(2026, 6, 11, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1020,
                column: "timestamp",
                value: new DateTime(2026, 6, 12, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1021,
                column: "timestamp",
                value: new DateTime(2026, 6, 13, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1022,
                column: "timestamp",
                value: new DateTime(2026, 6, 14, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1023,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1024,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1025,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1026,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1027,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1028,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 1029,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2000,
                column: "timestamp",
                value: new DateTime(2026, 5, 23, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2001,
                column: "timestamp",
                value: new DateTime(2026, 5, 24, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2002,
                column: "timestamp",
                value: new DateTime(2026, 5, 25, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2003,
                column: "timestamp",
                value: new DateTime(2026, 5, 26, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2004,
                column: "timestamp",
                value: new DateTime(2026, 5, 27, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2005,
                column: "timestamp",
                value: new DateTime(2026, 5, 28, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2006,
                column: "timestamp",
                value: new DateTime(2026, 5, 29, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2007,
                column: "timestamp",
                value: new DateTime(2026, 5, 30, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2008,
                column: "timestamp",
                value: new DateTime(2026, 5, 31, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2009,
                column: "timestamp",
                value: new DateTime(2026, 6, 1, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2010,
                column: "timestamp",
                value: new DateTime(2026, 6, 2, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2011,
                column: "timestamp",
                value: new DateTime(2026, 6, 3, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2012,
                column: "timestamp",
                value: new DateTime(2026, 6, 4, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2013,
                column: "timestamp",
                value: new DateTime(2026, 6, 5, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2014,
                column: "timestamp",
                value: new DateTime(2026, 6, 6, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2015,
                column: "timestamp",
                value: new DateTime(2026, 6, 7, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2016,
                column: "timestamp",
                value: new DateTime(2026, 6, 8, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2017,
                column: "timestamp",
                value: new DateTime(2026, 6, 9, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2018,
                column: "timestamp",
                value: new DateTime(2026, 6, 10, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2019,
                column: "timestamp",
                value: new DateTime(2026, 6, 11, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2020,
                column: "timestamp",
                value: new DateTime(2026, 6, 12, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2021,
                column: "timestamp",
                value: new DateTime(2026, 6, 13, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2022,
                column: "timestamp",
                value: new DateTime(2026, 6, 14, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2023,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2024,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2025,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2026,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2027,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2028,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 2029,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3000,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 18, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3001,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 19, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3002,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 20, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3003,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 21, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3004,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 22, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3005,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 23, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3006,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 0, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3007,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 1, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3008,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 2, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3009,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 3, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3010,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 4, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3011,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 5, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3012,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 6, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3013,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 7, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3014,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 8, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3015,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 9, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3016,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 10, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3017,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 11, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3018,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 12, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3019,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 13, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3020,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 14, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3021,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 15, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3022,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 16, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3023,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 17, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3024,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 18, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3025,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 19, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3026,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 20, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3027,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 21, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3028,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 22, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3029,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 23, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3030,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 0, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3031,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 1, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3032,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 2, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3033,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 3, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3034,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 4, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3035,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 5, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3036,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 6, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3037,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 7, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3038,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 8, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3039,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 9, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3040,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 10, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3041,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 11, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3042,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 12, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3043,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 13, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3044,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 14, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3045,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 15, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3046,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 16, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3047,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 17, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3048,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 18, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3049,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 19, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3050,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 20, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3051,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 21, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3052,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 22, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3053,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 23, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3054,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 0, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3055,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 1, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3056,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 2, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3057,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 3, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3058,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 4, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3059,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 5, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3060,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 6, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3061,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 7, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3062,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 8, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3063,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 9, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3064,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 10, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3065,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 11, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3066,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 12, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3067,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 13, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3068,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 14, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3069,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 15, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3070,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 16, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3071,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 17, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3072,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 18, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3073,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 19, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3074,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 20, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3075,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 21, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3076,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 22, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3077,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 23, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3078,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 0, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3079,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 1, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3080,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 2, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3081,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 3, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3082,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 4, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3083,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 5, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3084,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 6, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3085,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 7, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3086,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 8, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3087,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 9, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3088,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 10, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3089,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 11, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3090,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 12, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3091,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 13, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3092,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 14, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3093,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 15, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3094,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 16, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3095,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 17, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3096,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 18, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3097,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 19, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3098,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 20, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3099,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 21, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3100,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 22, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3101,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 23, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3102,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 0, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3103,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 1, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3104,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 2, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3105,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 3, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3106,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 4, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3107,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 5, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3108,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 6, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3109,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 7, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3110,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 8, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3111,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 9, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3112,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 10, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3113,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 11, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3114,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 12, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3115,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 13, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3116,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 14, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3117,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 15, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3118,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 16, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3119,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 17, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3120,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 18, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3121,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 19, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3122,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 20, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3123,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 21, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3124,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 22, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3125,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 23, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3126,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 0, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3127,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 1, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3128,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 2, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3129,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 3, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3130,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 4, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3131,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 5, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3132,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 6, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3133,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 7, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3134,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 8, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3135,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 9, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3136,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 10, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3137,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 11, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3138,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 12, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3139,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 13, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3140,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 14, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3141,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 15, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3142,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 16, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3143,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 17, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3144,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 18, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3145,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 19, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3146,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 20, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3147,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 21, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3148,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 22, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3149,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 23, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3150,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 0, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3151,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 1, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3152,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 2, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3153,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 3, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3154,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 4, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3155,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 5, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3156,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 6, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3157,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 7, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3158,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 8, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3159,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 9, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3160,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 10, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3161,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 11, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3162,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 12, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3163,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 13, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3164,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 14, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3165,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 15, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3166,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 16, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 3167,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 17, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4000,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 18, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4001,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 19, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4002,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 20, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4003,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 21, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4004,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 22, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4005,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 23, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4006,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 0, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4007,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 1, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4008,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 2, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4009,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 3, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4010,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 4, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4011,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 5, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4012,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 6, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4013,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 7, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4014,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 8, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4015,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 9, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4016,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 10, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4017,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 11, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4018,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 12, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4019,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 13, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4020,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 14, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4021,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 15, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4022,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 16, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4023,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 17, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4024,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 18, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4025,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 19, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4026,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 20, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4027,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 21, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4028,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 22, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4029,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 23, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4030,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 0, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4031,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 1, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4032,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 2, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4033,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 3, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4034,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 4, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4035,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 5, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4036,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 6, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4037,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 7, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4038,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 8, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4039,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 9, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4040,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 10, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4041,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 11, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4042,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 12, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4043,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 13, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4044,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 14, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4045,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 15, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4046,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 16, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4047,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 17, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4048,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 18, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4049,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 19, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4050,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 20, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4051,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 21, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4052,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 22, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4053,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 23, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4054,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 0, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4055,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 1, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4056,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 2, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4057,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 3, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4058,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 4, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4059,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 5, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4060,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 6, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4061,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 7, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4062,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 8, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4063,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 9, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4064,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 10, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4065,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 11, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4066,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 12, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4067,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 13, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4068,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 14, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4069,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 15, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4070,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 16, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4071,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 17, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4072,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 18, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4073,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 19, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4074,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 20, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4075,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 21, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4076,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 22, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4077,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 23, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4078,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 0, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4079,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 1, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4080,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 2, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4081,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 3, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4082,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 4, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4083,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 5, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4084,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 6, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4085,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 7, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4086,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 8, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4087,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 9, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4088,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 10, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4089,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 11, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4090,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 12, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4091,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 13, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4092,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 14, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4093,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 15, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4094,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 16, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4095,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 17, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4096,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 18, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4097,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 19, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4098,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 20, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4099,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 21, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4100,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 22, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4101,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 23, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4102,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 0, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4103,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 1, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4104,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 2, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4105,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 3, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4106,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 4, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4107,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 5, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4108,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 6, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4109,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 7, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4110,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 8, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4111,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 9, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4112,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 10, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4113,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 11, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4114,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 12, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4115,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 13, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4116,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 14, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4117,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 15, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4118,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 16, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4119,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 17, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4120,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 18, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4121,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 19, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4122,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 20, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4123,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 21, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4124,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 22, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4125,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 23, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4126,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 0, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4127,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 1, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4128,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 2, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4129,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 3, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4130,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 4, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4131,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 5, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4132,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 6, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4133,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 7, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4134,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 8, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4135,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 9, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4136,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 10, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4137,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 11, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4138,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 12, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4139,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 13, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4140,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 14, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4141,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 15, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4142,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 16, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4143,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 17, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4144,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 18, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4145,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 19, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4146,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 20, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4147,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 21, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4148,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 22, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4149,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 23, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4150,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 0, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4151,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 1, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4152,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 2, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4153,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 3, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4154,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 4, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4155,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 5, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4156,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 6, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4157,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 7, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4158,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 8, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4159,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 9, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4160,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 10, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4161,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 11, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4162,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 12, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4163,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 13, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4164,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 14, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4165,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 15, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4166,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 16, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 4167,
                column: "timestamp",
                value: new DateTime(2026, 6, 22, 17, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5000,
                column: "timestamp",
                value: new DateTime(2026, 5, 23, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5001,
                column: "timestamp",
                value: new DateTime(2026, 5, 24, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5002,
                column: "timestamp",
                value: new DateTime(2026, 5, 25, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5003,
                column: "timestamp",
                value: new DateTime(2026, 5, 26, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5004,
                column: "timestamp",
                value: new DateTime(2026, 5, 27, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5005,
                column: "timestamp",
                value: new DateTime(2026, 5, 28, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5006,
                column: "timestamp",
                value: new DateTime(2026, 5, 29, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5007,
                column: "timestamp",
                value: new DateTime(2026, 5, 30, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5008,
                column: "timestamp",
                value: new DateTime(2026, 5, 31, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5009,
                column: "timestamp",
                value: new DateTime(2026, 6, 1, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5010,
                column: "timestamp",
                value: new DateTime(2026, 6, 2, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5011,
                column: "timestamp",
                value: new DateTime(2026, 6, 3, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5012,
                column: "timestamp",
                value: new DateTime(2026, 6, 4, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5013,
                column: "timestamp",
                value: new DateTime(2026, 6, 5, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5014,
                column: "timestamp",
                value: new DateTime(2026, 6, 6, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5015,
                column: "timestamp",
                value: new DateTime(2026, 6, 7, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5016,
                column: "timestamp",
                value: new DateTime(2026, 6, 8, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5017,
                column: "timestamp",
                value: new DateTime(2026, 6, 9, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5018,
                column: "timestamp",
                value: new DateTime(2026, 6, 10, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5019,
                column: "timestamp",
                value: new DateTime(2026, 6, 11, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5020,
                column: "timestamp",
                value: new DateTime(2026, 6, 12, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5021,
                column: "timestamp",
                value: new DateTime(2026, 6, 13, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5022,
                column: "timestamp",
                value: new DateTime(2026, 6, 14, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5023,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5024,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5025,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5026,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5027,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5028,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 5029,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 18, 28, 43, 769, DateTimeKind.Utc).AddTicks(9096));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6000,
                column: "timestamp",
                value: new DateTime(2026, 6, 15, 18, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6001,
                column: "timestamp",
                value: new DateTime(2026, 6, 16, 18, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6002,
                column: "timestamp",
                value: new DateTime(2026, 6, 17, 18, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6003,
                column: "timestamp",
                value: new DateTime(2026, 6, 18, 18, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6004,
                column: "timestamp",
                value: new DateTime(2026, 6, 19, 18, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6005,
                column: "timestamp",
                value: new DateTime(2026, 6, 20, 18, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.UpdateData(
                table: "device_logs",
                keyColumn: "id",
                keyValue: 6006,
                column: "timestamp",
                value: new DateTime(2026, 6, 21, 18, 28, 43, 777, DateTimeKind.Utc).AddTicks(5542));

            migrationBuilder.CreateIndex(
                name: "IX_devices_project_id_floor_number_type",
                table: "devices",
                columns: new[] { "project_id", "floor_number", "type" },
                unique: true,
                filter: "unit_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_devices_project_id_unit_id_type",
                table: "devices",
                columns: new[] { "project_id", "unit_id", "type" },
                unique: true,
                filter: "unit_id IS NOT NULL");
        }
    }
}
