using Microsoft.EntityFrameworkCore.Migrations;

namespace WaterLog_Backend.Migrations
{
    public partial class LocalMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LocationSegments_Locations_LocationId",
                table: "LocationSegments");

            migrationBuilder.DropForeignKey(
                name: "FK_LocationSegments_Segments_SegmentId",
                table: "LocationSegments");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_LocationSegments_Locations_LocationId",
                table: "LocationSegments",
                column: "LocationId",
                principalTable: "Locations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LocationSegments_Segments_SegmentId",
                table: "LocationSegments",
                column: "SegmentId",
                principalTable: "Segments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
