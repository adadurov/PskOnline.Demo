using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace PskOnline.Server.DAL.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "App_BranchOffice",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    TenantId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    TimeZoneId = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_App_BranchOffice", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "App_Employee",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    TenantId = table.Column<Guid>(nullable: false),
                    UserId = table.Column<Guid>(nullable: true),
                    ExternalId = table.Column<string>(nullable: true),
                    FirstName = table.Column<string>(nullable: false),
                    LastName = table.Column<string>(nullable: false),
                    Patronymic = table.Column<string>(nullable: true),
                    BirthDate = table.Column<DateTime>(nullable: false),
                    Gender = table.Column<int>(nullable: false),
                    BranchOfficeId = table.Column<Guid>(nullable: false),
                    DepartmentId = table.Column<Guid>(nullable: false),
                    PositionId = table.Column<Guid>(nullable: false),
                    DateOfEmployment = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_App_Employee", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "App_Inspection",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    TenantId = table.Column<Guid>(nullable: false),
                    StartTimeUtc = table.Column<DateTime>(nullable: false),
                    FinishTimeUtc = table.Column<DateTime>(nullable: true),
                    MethodSetId = table.Column<string>(nullable: true),
                    MethodSetVersion = table.Column<string>(nullable: true),
                    MachineName = table.Column<string>(nullable: true),
                    InspectionType = table.Column<int>(nullable: false),
                    InspectionPlace = table.Column<int>(nullable: false),
                    DepartmentId = table.Column<Guid>(nullable: false),
                    BranchOfficeId = table.Column<Guid>(nullable: false),
                    EmployeeId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_App_Inspection", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "App_Tenant",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    Name = table.Column<string>(maxLength: 100, nullable: false),
                    Comment = table.Column<string>(nullable: true),
                    PrimaryContact_FullName = table.Column<string>(nullable: true),
                    PrimaryContact_Email = table.Column<string>(maxLength: 128, nullable: true),
                    PrimaryContact_OfficePhoneNumber = table.Column<string>(nullable: true),
                    PrimaryContact_MobilePhoneNumber = table.Column<string>(nullable: true),
                    PrimaryContact_StreetAddress = table.Column<string>(maxLength: 256, nullable: true),
                    PrimaryContact_City = table.Column<string>(maxLength: 128, nullable: true),
                    PrimaryContact_Comment = table.Column<string>(nullable: true),
                    ServiceDetails_ServiceExpireDate = table.Column<DateTime>(nullable: false),
                    ServiceDetails_ServiceMaxUsers = table.Column<int>(nullable: false),
                    ServiceDetails_ServiceMaxEmployees = table.Column<int>(nullable: false),
                    ServiceDetails_ServiceMaxStorageMegabytes = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_App_Tenant", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "App_Department",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    TenantId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: false),
                    BranchOfficeId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_App_Department", x => x.Id);
                    table.ForeignKey(
                        name: "FK_App_Department_App_BranchOffice_BranchOfficeId",
                        column: x => x.BranchOfficeId,
                        principalTable: "App_BranchOffice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "App_Position",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    TenantId = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(maxLength: 256, nullable: false),
                    BranchOfficeId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_App_Position", x => x.Id);
                    table.ForeignKey(
                        name: "FK_App_Position_App_BranchOffice_BranchOfficeId",
                        column: x => x.BranchOfficeId,
                        principalTable: "App_BranchOffice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "App_Test",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CreatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedBy = table.Column<string>(maxLength: 256, nullable: true),
                    UpdatedDate = table.Column<DateTime>(nullable: false),
                    CreatedDate = table.Column<DateTime>(nullable: false),
                    TenantId = table.Column<Guid>(nullable: false),
                    InspectionId = table.Column<Guid>(nullable: false),
                    MethodId = table.Column<string>(nullable: false),
                    MethodVersion = table.Column<string>(nullable: true),
                    StartTimeUtc = table.Column<DateTime>(nullable: false),
                    FinishTimeUtc = table.Column<DateTime>(nullable: false),
                    EmployeeId = table.Column<Guid>(nullable: false),
                    DepartmentId = table.Column<Guid>(nullable: false),
                    BranchOfficeId = table.Column<Guid>(nullable: false),
                    MethodRawDataJson = table.Column<string>(nullable: true),
                    MethodProcessedDataJson = table.Column<string>(nullable: true),
                    Comment = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_App_Test", x => x.Id);
                    table.ForeignKey(
                        name: "FK_App_Test_App_Inspection_InspectionId",
                        column: x => x.InspectionId,
                        principalTable: "App_Inspection",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_App_BranchOffice_TenantId",
                table: "App_BranchOffice",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_App_Department_BranchOfficeId",
                table: "App_Department",
                column: "BranchOfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_App_Department_TenantId",
                table: "App_Department",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_App_Employee_BranchOfficeId",
                table: "App_Employee",
                column: "BranchOfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_App_Employee_DepartmentId",
                table: "App_Employee",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_App_Employee_TenantId",
                table: "App_Employee",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_App_Employee_UserId",
                table: "App_Employee",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_App_Inspection_BranchOfficeId",
                table: "App_Inspection",
                column: "BranchOfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_App_Inspection_DepartmentId",
                table: "App_Inspection",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_App_Inspection_EmployeeId",
                table: "App_Inspection",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_App_Inspection_FinishTimeUtc",
                table: "App_Inspection",
                column: "FinishTimeUtc");

            migrationBuilder.CreateIndex(
                name: "IX_App_Inspection_MethodSetId",
                table: "App_Inspection",
                column: "MethodSetId");

            migrationBuilder.CreateIndex(
                name: "IX_App_Inspection_StartTimeUtc",
                table: "App_Inspection",
                column: "StartTimeUtc");

            migrationBuilder.CreateIndex(
                name: "IX_App_Inspection_TenantId",
                table: "App_Inspection",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_App_Position_BranchOfficeId",
                table: "App_Position",
                column: "BranchOfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_App_Position_TenantId",
                table: "App_Position",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_App_Tenant_Name",
                table: "App_Tenant",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_App_Test_BranchOfficeId",
                table: "App_Test",
                column: "BranchOfficeId");

            migrationBuilder.CreateIndex(
                name: "IX_App_Test_DepartmentId",
                table: "App_Test",
                column: "DepartmentId");

            migrationBuilder.CreateIndex(
                name: "IX_App_Test_EmployeeId",
                table: "App_Test",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_App_Test_FinishTimeUtc",
                table: "App_Test",
                column: "FinishTimeUtc");

            migrationBuilder.CreateIndex(
                name: "IX_App_Test_InspectionId",
                table: "App_Test",
                column: "InspectionId");

            migrationBuilder.CreateIndex(
                name: "IX_App_Test_StartTimeUtc",
                table: "App_Test",
                column: "StartTimeUtc");

            migrationBuilder.CreateIndex(
                name: "IX_App_Test_TenantId",
                table: "App_Test",
                column: "TenantId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "App_Department");

            migrationBuilder.DropTable(
                name: "App_Employee");

            migrationBuilder.DropTable(
                name: "App_Position");

            migrationBuilder.DropTable(
                name: "App_Tenant");

            migrationBuilder.DropTable(
                name: "App_Test");

            migrationBuilder.DropTable(
                name: "App_BranchOffice");

            migrationBuilder.DropTable(
                name: "App_Inspection");
        }
    }
}
