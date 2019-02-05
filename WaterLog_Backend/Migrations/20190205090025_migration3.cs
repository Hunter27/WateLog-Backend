using Microsoft.EntityFrameworkCore.Migrations;

namespace WaterLog_Backend.Migrations
{
    public partial class migration3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Value",
                table: "SegmentEvents",
                newName: "FlowOut");

            migrationBuilder.AddColumn<double>(
                name: "FlowIn",
                table: "SegmentEvents",
                nullable: false,
                defaultValue: 0.0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FlowIn",
                table: "SegmentEvents");

            migrationBuilder.RenameColumn(
                name: "FlowOut",
                table: "SegmentEvents",
                newName: "Value");
        }
    }
}
