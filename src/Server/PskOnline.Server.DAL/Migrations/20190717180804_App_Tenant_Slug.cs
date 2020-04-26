using Microsoft.EntityFrameworkCore.Migrations;

namespace PskOnline.Server.DAL.Migrations
{
    public partial class App_Tenant_Slug : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "App_Tenant",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_App_Tenant_Slug",
                table: "App_Tenant",
                column: "Slug");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_App_Tenant_Slug",
                table: "App_Tenant");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "App_Tenant");
        }
    }
}
