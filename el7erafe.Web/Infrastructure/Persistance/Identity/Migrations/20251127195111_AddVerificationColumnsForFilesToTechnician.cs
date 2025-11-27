using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistance.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddVerificationColumnsForFilesToTechnician : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsCriminalHistoryVerified",
                table: "Technicians",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsNationalIdBackVerified",
                table: "Technicians",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsNationalIdFrontVerified",
                table: "Technicians",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Rejection_Count",
                table: "Technicians",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCriminalHistoryVerified",
                table: "Technicians");

            migrationBuilder.DropColumn(
                name: "IsNationalIdBackVerified",
                table: "Technicians");

            migrationBuilder.DropColumn(
                name: "IsNationalIdFrontVerified",
                table: "Technicians");

            migrationBuilder.DropColumn(
                name: "Rejection_Count",
                table: "Technicians");
        }
    }
}
