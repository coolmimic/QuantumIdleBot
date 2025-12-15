using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuantumIdleWEB.Migrations
{
    /// <inheritdoc />
    public partial class AddTgMsgIdToBetOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PushAlerts",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PushOrders",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "TelegramChatId",
                table: "Users",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "TgMsgId",
                table: "BetOrders",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PushAlerts",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PushOrders",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TelegramChatId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TgMsgId",
                table: "BetOrders");
        }
    }
}
