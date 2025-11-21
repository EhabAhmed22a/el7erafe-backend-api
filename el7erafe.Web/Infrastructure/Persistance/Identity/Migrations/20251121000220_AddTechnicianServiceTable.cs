using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistance.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddTechnicianServiceTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServiceId",
                table: "Technicians",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TechnicianService",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NameEn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnicianService", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Technicians_ServiceId",
                table: "Technicians",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_Technicians_TechnicianService_ServiceId",
                table: "Technicians",
                column: "ServiceId",
                principalTable: "TechnicianService",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Technicians_TechnicianService_ServiceId",
                table: "Technicians");

            migrationBuilder.DropTable(
                name: "TechnicianService");

            migrationBuilder.DropIndex(
                name: "IX_Technicians_ServiceId",
                table: "Technicians");

            migrationBuilder.DropColumn(
                name: "ServiceId",
                table: "Technicians");
        }
    }
}
