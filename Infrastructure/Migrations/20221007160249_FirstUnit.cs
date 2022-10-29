using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FirstUnit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Units",
                columns: new[] { "Id", "ExpiryDate", "FridgeId", "ItemId", "Quantity" },
                values: new object[] { 1, new DateTime(2022, 3, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 1, 3.0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Units",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
