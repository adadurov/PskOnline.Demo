using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PskOnline.Server.DAL.Migrations
{
    public partial class App_Use_DateTimeOffset : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_App_Test_FinishTimeUtc",
                table: "App_Test");

            migrationBuilder.DropIndex(
                name: "IX_App_Test_StartTimeUtc",
                table: "App_Test");

            migrationBuilder.DropIndex(
                name: "IX_App_Inspection_FinishTimeUtc",
                table: "App_Inspection");

            migrationBuilder.DropIndex(
                name: "IX_App_Inspection_StartTimeUtc",
                table: "App_Inspection");

            migrationBuilder.DropColumn(
                name: "FinishTimeUtc",
                table: "App_Test");

            migrationBuilder.DropColumn(
                name: "StartTimeUtc",
                table: "App_Test");

            migrationBuilder.DropColumn(
                name: "FinishTimeUtc",
                table: "App_Inspection");

            migrationBuilder.DropColumn(
                name: "StartTimeUtc",
                table: "App_Inspection");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FinishTime",
                table: "App_Test",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StartTime",
                table: "App_Test",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "FinishTime",
                table: "App_Inspection",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "StartTime",
                table: "App_Inspection",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateIndex(
                name: "IX_App_Test_FinishTime",
                table: "App_Test",
                column: "FinishTime");

            migrationBuilder.CreateIndex(
                name: "IX_App_Test_StartTime",
                table: "App_Test",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX_App_Inspection_FinishTime",
                table: "App_Inspection",
                column: "FinishTime");

            migrationBuilder.CreateIndex(
                name: "IX_App_Inspection_StartTime",
                table: "App_Inspection",
                column: "StartTime");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_App_Test_FinishTime",
                table: "App_Test");

            migrationBuilder.DropIndex(
                name: "IX_App_Test_StartTime",
                table: "App_Test");

            migrationBuilder.DropIndex(
                name: "IX_App_Inspection_FinishTime",
                table: "App_Inspection");

            migrationBuilder.DropIndex(
                name: "IX_App_Inspection_StartTime",
                table: "App_Inspection");

            migrationBuilder.DropColumn(
                name: "FinishTime",
                table: "App_Test");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "App_Test");

            migrationBuilder.DropColumn(
                name: "FinishTime",
                table: "App_Inspection");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "App_Inspection");

            migrationBuilder.AddColumn<DateTime>(
                name: "FinishTimeUtc",
                table: "App_Test",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTimeUtc",
                table: "App_Test",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FinishTimeUtc",
                table: "App_Inspection",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTimeUtc",
                table: "App_Inspection",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.CreateIndex(
                name: "IX_App_Test_FinishTimeUtc",
                table: "App_Test",
                column: "FinishTimeUtc");

            migrationBuilder.CreateIndex(
                name: "IX_App_Test_StartTimeUtc",
                table: "App_Test",
                column: "StartTimeUtc");

            migrationBuilder.CreateIndex(
                name: "IX_App_Inspection_FinishTimeUtc",
                table: "App_Inspection",
                column: "FinishTimeUtc");

            migrationBuilder.CreateIndex(
                name: "IX_App_Inspection_StartTimeUtc",
                table: "App_Inspection",
                column: "StartTimeUtc");
        }
    }
}
