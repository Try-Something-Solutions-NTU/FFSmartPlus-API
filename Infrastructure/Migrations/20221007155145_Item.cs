using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Item : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Items",
                columns: new[] { "ID", "DesiredStock", "Name", "RestockTime", "UnitDesc" },
                values: new object[] { 1, 1.0, "Tomatoes", 3.0, "Per Tomato" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Items",
                keyColumn: "ID",
                keyValue: 1);
        }
    }
}
