using Microsoft.EntityFrameworkCore.Migrations;

namespace WaterLog_Backend.Migrations
{
    public partial class faultCount2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "FaultCount",
                table: "Segments",
                nullable: false,
                oldClrType: typeof(double));

            migrationBuilder.AlterColumn<int>(
                name: "FaultCount",
                table: "Monitors",
                nullable: false,
                oldClrType: typeof(double));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "FaultCount",
                table: "Segments",
                nullable: false,
                oldClrType: typeof(int));

            migrationBuilder.AlterColumn<double>(
                name: "FaultCount",
                table: "Monitors",
                nullable: false,
                oldClrType: typeof(int));
        }
    }
}
