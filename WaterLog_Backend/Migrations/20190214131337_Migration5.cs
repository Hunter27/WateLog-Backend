using Microsoft.EntityFrameworkCore.Migrations;

namespace WaterLog_Backend.Migrations
{
    public partial class Migration5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Level_Status",
                table: "TankLevels",
                newName: "LevelStatus");

            migrationBuilder.RenameColumn(
                name: "Tank_Id",
                table: "TankLevels",
                newName: "TankId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LevelStatus",
                table: "TankLevels",
                newName: "Level_Status");

            migrationBuilder.RenameColumn(
                name: "TankId",
                table: "TankLevels",
                newName: "Tank_Id");
        }
    }
}
