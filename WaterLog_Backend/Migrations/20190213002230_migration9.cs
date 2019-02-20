using Microsoft.EntityFrameworkCore.Migrations;

namespace WaterLog_Backend.Migrations
{
    public partial class migration9 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SegmentEventsId",
                table: "SegmentLeaks",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SegmentLeaks_SegmentEventsId",
                table: "SegmentLeaks",
                column: "SegmentEventsId");

            migrationBuilder.AddForeignKey(
                name: "FK_SegmentLeaks_SegmentEvents_SegmentEventsId",
                table: "SegmentLeaks",
                column: "SegmentEventsId",
                principalTable: "SegmentEvents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SegmentLeaks_SegmentEvents_SegmentEventsId",
                table: "SegmentLeaks");

            migrationBuilder.DropIndex(
                name: "IX_SegmentLeaks_SegmentEventsId",
                table: "SegmentLeaks");

            migrationBuilder.DropColumn(
                name: "SegmentEventsId",
                table: "SegmentLeaks");
        }
    }
}
