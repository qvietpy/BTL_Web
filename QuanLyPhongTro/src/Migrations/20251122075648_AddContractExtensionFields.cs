using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuanLyPhongTro.src.Migrations
{
    /// <inheritdoc />
    public partial class AddContractExtensionFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExtensionStatus",
                table: "Contract",
                type: "nvarchar(15)",
                maxLength: 15,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequestedExtensionMonths",
                table: "Contract",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExtensionStatus",
                table: "Contract");

            migrationBuilder.DropColumn(
                name: "RequestedExtensionMonths",
                table: "Contract");
        }
    }
}
