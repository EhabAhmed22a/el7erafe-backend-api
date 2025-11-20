using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistance.Identity.Migrations
{
    /// <inheritdoc />
    public partial class CaptureModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_City_Governorate_GovernorateId",
                table: "City");

            migrationBuilder.DropForeignKey(
                name: "FK_Technicians_City_CityId",
                table: "Technicians");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Governorate",
                table: "Governorate");

            migrationBuilder.DropPrimaryKey(
                name: "PK_City",
                table: "City");

            migrationBuilder.RenameTable(
                name: "Governorate",
                newName: "Governorates");

            migrationBuilder.RenameTable(
                name: "City",
                newName: "Cities");

            migrationBuilder.RenameIndex(
                name: "IX_City_GovernorateId",
                table: "Cities",
                newName: "IX_Cities_GovernorateId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Governorates",
                table: "Governorates",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Cities",
                table: "Cities",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Cities_Governorates_GovernorateId",
                table: "Cities",
                column: "GovernorateId",
                principalTable: "Governorates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Technicians_Cities_CityId",
                table: "Technicians",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cities_Governorates_GovernorateId",
                table: "Cities");

            migrationBuilder.DropForeignKey(
                name: "FK_Technicians_Cities_CityId",
                table: "Technicians");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Governorates",
                table: "Governorates");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Cities",
                table: "Cities");

            migrationBuilder.RenameTable(
                name: "Governorates",
                newName: "Governorate");

            migrationBuilder.RenameTable(
                name: "Cities",
                newName: "City");

            migrationBuilder.RenameIndex(
                name: "IX_Cities_GovernorateId",
                table: "City",
                newName: "IX_City_GovernorateId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Governorate",
                table: "Governorate",
                column: "Id");

            migrationBuilder.AddPrimaryKey(
                name: "PK_City",
                table: "City",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_City_Governorate_GovernorateId",
                table: "City",
                column: "GovernorateId",
                principalTable: "Governorate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Technicians_City_CityId",
                table: "Technicians",
                column: "CityId",
                principalTable: "City",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
