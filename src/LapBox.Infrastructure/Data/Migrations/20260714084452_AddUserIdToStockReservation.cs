using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LapBox.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToStockReservation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "OrderId",
                table: "StockReservations",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "StockReservations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_StockReservations_UserId",
                table: "StockReservations",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_StockReservations_UserId",
                table: "StockReservations");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "StockReservations");

            migrationBuilder.AlterColumn<Guid>(
                name: "OrderId",
                table: "StockReservations",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
