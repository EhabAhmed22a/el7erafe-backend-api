using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistance.Identity.Migrations
{
    /// <inheritdoc />
    public partial class ChangeTechServiceTableName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Technicians_TechnicianService_ServiceId",
                table: "Technicians");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TechnicianService",
                table: "TechnicianService");

            migrationBuilder.RenameTable(
                name: "TechnicianService",
                newName: "TechnicianServices");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TechnicianServices",
                table: "TechnicianServices",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Technicians_TechnicianServices_ServiceId",
                table: "Technicians",
                column: "ServiceId",
                principalTable: "TechnicianServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Technicians_TechnicianServices_ServiceId",
                table: "Technicians");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TechnicianServices",
                table: "TechnicianServices");

            migrationBuilder.RenameTable(
                name: "TechnicianServices",
                newName: "TechnicianService");

            migrationBuilder.AddPrimaryKey(
                name: "PK_TechnicianService",
                table: "TechnicianService",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Technicians_TechnicianService_ServiceId",
                table: "Technicians",
                column: "ServiceId",
                principalTable: "TechnicianService",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
