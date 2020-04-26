using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PskOnline.Server.Plugins.RusHydro.DAL.Migrations
{
    public partial class RusHydro_Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RusHydro_Summary",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    TenantId = table.Column<Guid>(nullable: false),
                    InspectionId = table.Column<Guid>(nullable: false),
                    DepartmentId = table.Column<Guid>(nullable: false),
                    BranchOfficeId = table.Column<Guid>(nullable: false),
                    EmployeeId = table.Column<Guid>(nullable: false),
                    CompletionTime = table.Column<DateTimeOffset>(nullable: false),
                    UpdateDate = table.Column<DateTimeOffset>(nullable: false),
                    SummaryDocument = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RusHydro_Summary", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RusHydro_Summary_BranchOfficeId",
                table: "RusHydro_Summary",
                column: "BranchOfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_RusHydro_Summary_CompletionTime",
                table: "RusHydro_Summary",
                column: "CompletionTime");

            migrationBuilder.CreateIndex(
                name: "IX_RusHydro_Summary_DepartmentId",
                table: "RusHydro_Summary",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_RusHydro_Summary_EmployeeId",
                table: "RusHydro_Summary",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_RusHydro_Summary_InspectionId",
                table: "RusHydro_Summary",
                column: "InspectionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RusHydro_Summary_TenantId",
                table: "RusHydro_Summary",
                column: "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RusHydro_Summary");
        }
    }
}
