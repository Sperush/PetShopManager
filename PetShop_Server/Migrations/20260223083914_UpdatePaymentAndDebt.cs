using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PetShop_Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePaymentAndDebt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "AmountPaid",
                table: "Booking",
                type: "decimal(18,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountPaid",
                table: "Booking");
        }
    }
}
