using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FortuneGacha.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddGachaPointsAndRarity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GachaPoints",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Rarity",
                table: "DailyFortunes",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GachaPoints",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Rarity",
                table: "DailyFortunes");
        }
    }
}
