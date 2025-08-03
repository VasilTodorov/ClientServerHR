using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClientServerHR.Migrations
{
    /// <inheritdoc />
    public partial class IBAN_Only_migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EncryptedIban",
                table: "Employees");

            migrationBuilder.AddColumn<string>(
                name: "IBAN",
                table: "Employees",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IBAN",
                table: "Employees");

            migrationBuilder.AddColumn<string>(
                name: "EncryptedIban",
                table: "Employees",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }
    }
}
