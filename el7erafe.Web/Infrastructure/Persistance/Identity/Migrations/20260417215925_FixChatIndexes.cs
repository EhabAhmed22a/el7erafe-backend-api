using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistance.Identity.Migrations
{
    /// <inheritdoc />
    public partial class FixChatIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chats_Client_Technician",
                table: "Chats");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Chats_Client_Technician",
                table: "Chats",
                columns: new[] { "ClientId", "TechnicianId" },
                unique: true);
        }
    }
}
