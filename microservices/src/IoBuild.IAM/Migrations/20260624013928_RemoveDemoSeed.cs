using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IoBuild.IAM.Migrations
{
    /// <inheritdoc />
    public partial class RemoveDemoSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "Id",
                keyValue: 2);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "Id", "email", "password_hash", "role" },
                values: new object[,]
                {
                    { 1, "builder@iobuilt.com", "$2a$11$8KE/oWpBDoA5ut.ICVRHv.VYZO8QhbfKnNh7gXyA9Ri9v8HPwdYZG", "builder" },
                    { 2, "owner@iobuilt.com", "$2a$11$334wilQOL2RMAkza.huR3uiOlVPnLzRakC7WegfHjFng7I6gDFhF2", "owner" }
                });
        }
    }
}
