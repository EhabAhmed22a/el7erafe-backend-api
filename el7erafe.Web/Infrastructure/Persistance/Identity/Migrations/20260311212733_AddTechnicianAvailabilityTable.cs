using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistance.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddTechnicianAvailabilityTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TechnicianAvailabilities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TechnicianId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: true),
                    FromTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    ToTime = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnicianAvailabilities", x => x.Id);
                    table.CheckConstraint("CK_TechnicianAvailability_TimeRange", "[FromTime] < [ToTime]");
                    table.ForeignKey(
                        name: "FK_TechnicianAvailabilities_Technicians_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TechnicianAvailabilities_TechnicianId_DayOfWeek",
                table: "TechnicianAvailabilities",
                columns: new[] { "TechnicianId", "DayOfWeek" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TechnicianAvailabilities");
        }
    }
}
