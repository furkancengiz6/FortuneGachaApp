using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FortuneGacha.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPushToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PushToken",
                table: "Users",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PushToken",
                table: "Users");
        }
    }
}
