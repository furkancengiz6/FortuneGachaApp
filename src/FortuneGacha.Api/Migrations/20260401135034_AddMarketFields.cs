using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FortuneGacha.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsForSale",
                table: "DailyFortunes",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Price",
                table: "DailyFortunes",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsForSale",
                table: "DailyFortunes");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "DailyFortunes");
        }
    }
}
