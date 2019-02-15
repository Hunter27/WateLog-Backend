using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace WaterLog_Backend.Migrations
{
    public partial class Migration3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Readings_Monitors_SenseID",
                table: "Readings");

            migrationBuilder.DropForeignKey(
                name: "FK_SegmentEvents_Segments_SegmentId",
                table: "SegmentEvents");

            migrationBuilder.DropIndex(
                name: "IX_SegmentEvents_SegmentId",
                table: "SegmentEvents");

            migrationBuilder.DropIndex(
                name: "IX_Readings_SenseID",
                table: "Readings");

            migrationBuilder.RenameColumn(
                name: "Value",
                table: "SegmentEvents",
                newName: "FlowOut");

            migrationBuilder.RenameColumn(
                name: "SegmentId",
                table: "SegmentEvents",
                newName: "SegmentsId");

            migrationBuilder.RenameColumn(
                name: "SenseID",
                table: "Readings",
                newName: "MonitorsId");

            migrationBuilder.AddColumn<double>(
                name: "FlowIn",
                table: "SegmentEvents",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "SegmentLeaks",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    SegmentsId = table.Column<int>(nullable: false),
                    Severity = table.Column<string>(nullable: true),
                    OriginalTimeStamp = table.Column<DateTime>(nullable: false),
                    LatestTimeStamp = table.Column<DateTime>(nullable: false),
                    ResolvedStatus = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SegmentLeaks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TankLevels",
                columns: table => new
                {
                    Tank_Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Percentage = table.Column<int>(nullable: false),
                    Level_Status = table.Column<string>(nullable: true),
                    Instruction = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TankLevels", x => x.Tank_Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SegmentLeaks");

            migrationBuilder.DropTable(
                name: "TankLevels");

            migrationBuilder.DropColumn(
                name: "FlowIn",
                table: "SegmentEvents");

            migrationBuilder.RenameColumn(
                name: "SegmentsId",
                table: "SegmentEvents",
                newName: "SegmentId");

            migrationBuilder.RenameColumn(
                name: "FlowOut",
                table: "SegmentEvents",
                newName: "Value");

            migrationBuilder.RenameColumn(
                name: "MonitorsId",
                table: "Readings",
                newName: "SenseID");

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
        }
    }
}
