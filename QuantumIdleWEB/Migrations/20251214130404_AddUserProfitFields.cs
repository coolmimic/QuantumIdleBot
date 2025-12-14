using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuantumIdleWEB.Migrations
{
    /// <inheritdoc />
    public partial class AddUserProfitFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Profit",
                table: "Users",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SimProfit",
                table: "Users",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "SimTurnover",
                table: "Users",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Turnover",
                table: "Users",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Profit",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SimProfit",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SimTurnover",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Turnover",
                table: "Users");
        }
    }
}
