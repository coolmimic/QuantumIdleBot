using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuantumIdleWEB.Migrations
{
    /// <inheritdoc />
    public partial class AddSchemeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Schemes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    SchemeId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TgGroupName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TgGroupId = table.Column<long>(type: "bigint", nullable: false),
                    GameType = table.Column<int>(type: "int", nullable: false),
                    PlayMode = table.Column<int>(type: "int", nullable: false),
                    OddsType = table.Column<int>(type: "int", nullable: false),
                    PositionLst = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OddsConfig = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DrawRule = table.Column<int>(type: "int", nullable: false),
                    DrawRuleConfig = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnableStopProfitLoss = table.Column<bool>(type: "bit", nullable: false),
                    StopProfitAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    StopLossAmount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    CreateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdateTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schemes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Schemes_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Schemes_UserId_SchemeId",
                table: "Schemes",
                columns: new[] { "UserId", "SchemeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Schemes");
        }
    }
}
