using Microsoft.EntityFrameworkCore.Migrations;

namespace PskOnline.Server.Plugins.RusHydro.DAL.Migrations
{
    public partial class RH_Add_Shift_Ids : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DepartmentShiftReportId",
                table: "RusHydro_Summary",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ShiftAbsoluteIndex",
                table: "RusHydro_Summary",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_RusHydro_Summary_ShiftAbsoluteIndex",
                table: "RusHydro_Summary",
                column: "ShiftAbsoluteIndex");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RusHydro_Summary_ShiftAbsoluteIndex",
                table: "RusHydro_Summary");

            migrationBuilder.DropColumn(
                name: "DepartmentShiftReportId",
                table: "RusHydro_Summary");

            migrationBuilder.DropColumn(
                name: "ShiftAbsoluteIndex",
                table: "RusHydro_Summary");
        }
    }
}
