using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LapBox.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCartReservationStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Carts",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Active");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Carts");
        }
    }
}
