using Microsoft.EntityFrameworkCore.Migrations;

namespace WaterLog_Backend.Migrations
{
    public partial class Migration2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SenseIDIn",
                table: "Segments",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SenseIDOut",
                table: "Segments",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SenseIDIn",
                table: "Segments");

            migrationBuilder.DropColumn(
                name: "SenseIDOut",
                table: "Segments");
        }
    }
}
