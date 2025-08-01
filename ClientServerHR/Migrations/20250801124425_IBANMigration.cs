using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientServerHR.Migrations
{
    /// <inheritdoc />
    public partial class IBANMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EncryptedIban",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncryptedIban",
                table: "Employees");
        }
    }
}
