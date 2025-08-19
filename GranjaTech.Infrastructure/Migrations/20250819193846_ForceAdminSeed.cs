using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GranjaTech.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ForceAdminSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "Usuarios",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "Lotes",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "Granjas",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "Granjas");
        }
    }
}
