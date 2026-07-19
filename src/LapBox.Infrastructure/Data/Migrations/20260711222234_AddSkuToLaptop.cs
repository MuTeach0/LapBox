using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LapBox.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSkuToLaptop : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Sku",
                table: "Laptops",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Laptops_Sku",
                table: "Laptops",
                column: "Sku",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Laptops_Sku",
                table: "Laptops");

            migrationBuilder.DropColumn(
                name: "Sku",
                table: "Laptops");
        }
    }
}
