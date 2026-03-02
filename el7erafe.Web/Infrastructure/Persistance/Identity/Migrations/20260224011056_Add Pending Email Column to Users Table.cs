using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistance.Identity.Migrations
{
    /// <inheritdoc />
    public partial class AddPendingEmailColumntoUsersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PendingEmail",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PendingEmail",
                table: "Users");
        }
    }
}
