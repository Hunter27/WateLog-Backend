using Microsoft.EntityFrameworkCore.Migrations;

namespace WaterLog_Backend.Migrations
{
    public partial class migration7 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Readings_Monitors_SenseID",
                table: "Readings");

            migrationBuilder.DropForeignKey(
                name: "FK_SegmentEvents_Segments_SegmentId",
                table: "SegmentEvents");

            migrationBuilder.DropForeignKey(
                name: "FK_SegmentLeaks_Segments_SegmentId",
                table: "SegmentLeaks");

            migrationBuilder.DropIndex(
                name: "IX_SegmentLeaks_SegmentId",
                table: "SegmentLeaks");

            migrationBuilder.DropIndex(
                name: "IX_SegmentEvents_SegmentId",
                table: "SegmentEvents");

            migrationBuilder.DropIndex(
                name: "IX_Readings_SenseID",
                table: "Readings");

            migrationBuilder.RenameColumn(
                name: "SegmentId",
                table: "SegmentLeaks",
                newName: "SegmentsId");

            migrationBuilder.RenameColumn(
                name: "SegmentId",
                table: "SegmentEvents",
                newName: "SegmentsId");

            migrationBuilder.RenameColumn(
                name: "SenseID",
                table: "Readings",
                newName: "MonitorsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SegmentsId",
                table: "SegmentLeaks",
                newName: "SegmentId");

            migrationBuilder.RenameColumn(
                name: "SegmentsId",
                table: "SegmentEvents",
                newName: "SegmentId");

            migrationBuilder.RenameColumn(
                name: "MonitorsId",
                table: "Readings",
                newName: "SenseID");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentLeaks_SegmentId",
                table: "SegmentLeaks",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_SegmentEvents_SegmentId",
                table: "SegmentEvents",
                column: "SegmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Readings_SenseID",
                table: "Readings",
                column: "SenseID");

            migrationBuilder.AddForeignKey(
                name: "FK_Readings_Monitors_SenseID",
                table: "Readings",
                column: "SenseID",
                principalTable: "Monitors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SegmentEvents_Segments_SegmentId",
                table: "SegmentEvents",
                column: "SegmentId",
                principalTable: "Segments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SegmentLeaks_Segments_SegmentId",
                table: "SegmentLeaks",
                column: "SegmentId",
                principalTable: "Segments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
