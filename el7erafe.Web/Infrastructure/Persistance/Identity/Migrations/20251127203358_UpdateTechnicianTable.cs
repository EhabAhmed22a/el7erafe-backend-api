using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistance.Identity.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTechnicianTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rejections_TechnicianId",
                table: "Rejections");

            migrationBuilder.AddColumn<int>(
                name: "RejectionId",
                table: "Technicians",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Rejections_TechnicianId",
                table: "Rejections",
                column: "TechnicianId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Rejections_TechnicianId",
                table: "Rejections");

            migrationBuilder.DropColumn(
                name: "RejectionId",
                table: "Technicians");

            migrationBuilder.CreateIndex(
                name: "IX_Rejections_TechnicianId",
                table: "Rejections",
                column: "TechnicianId");
        }
    }
}
