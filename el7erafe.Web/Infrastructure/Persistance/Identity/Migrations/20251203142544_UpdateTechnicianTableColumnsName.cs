using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistance.Identity.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTechnicianTableColumnsName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsNationalIdFrontVerified",
                table: "Technicians",
                newName: "IsNationalIdFrontRejected");

            migrationBuilder.RenameColumn(
                name: "IsNationalIdBackVerified",
                table: "Technicians",
                newName: "IsNationalIdBackRejected");

            migrationBuilder.RenameColumn(
                name: "IsCriminalHistoryVerified",
                table: "Technicians",
                newName: "IsCriminalHistoryRejected");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsNationalIdFrontRejected",
                table: "Technicians",
                newName: "IsNationalIdFrontVerified");

            migrationBuilder.RenameColumn(
                name: "IsNationalIdBackRejected",
                table: "Technicians",
                newName: "IsNationalIdBackVerified");

            migrationBuilder.RenameColumn(
                name: "IsCriminalHistoryRejected",
                table: "Technicians",
                newName: "IsCriminalHistoryVerified");
        }
    }
}
