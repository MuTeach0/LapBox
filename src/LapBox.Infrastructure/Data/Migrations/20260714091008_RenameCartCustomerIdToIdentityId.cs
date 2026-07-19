using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LapBox.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenameCartCustomerIdToIdentityId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_Customers_CustomerId",
                table: "Carts");

            migrationBuilder.RenameColumn(
                name: "CustomerId",
                table: "Carts",
                newName: "IdentityId");

            migrationBuilder.RenameIndex(
                name: "IX_Carts_CustomerId",
                table: "Carts",
                newName: "IX_Carts_IdentityId");

            // Existing carts contain Customers.Id values. Replace them with the
            // corresponding Customers.IdentityId before enforcing the new FK.
            migrationBuilder.Sql("""
                UPDATE carts
                SET carts.IdentityId = customers.IdentityId
                FROM Carts AS carts
                INNER JOIN Customers AS customers ON carts.IdentityId = customers.Id;
                """);

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_AspNetUsers_IdentityId",
                table: "Carts",
                column: "IdentityId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Carts_AspNetUsers_IdentityId",
                table: "Carts");

            migrationBuilder.RenameColumn(
                name: "IdentityId",
                table: "Carts",
                newName: "CustomerId");

            migrationBuilder.RenameIndex(
                name: "IX_Carts_IdentityId",
                table: "Carts",
                newName: "IX_Carts_CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Carts_Customers_CustomerId",
                table: "Carts",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
