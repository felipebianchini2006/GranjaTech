using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GranjaTech.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class mortalidade : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcaoTomada",
                table: "RegistrosMortalidade");

            migrationBuilder.DropColumn(
                name: "AvesVivas",
                table: "RegistrosMortalidade");

            migrationBuilder.DropColumn(
                name: "IdadeDias",
                table: "RegistrosMortalidade");

            migrationBuilder.DropColumn(
                name: "PesoMedioMortas",
                table: "RegistrosMortalidade");

            migrationBuilder.RenameColumn(
                name: "QuantidadeMortas",
                table: "RegistrosMortalidade",
                newName: "Quantidade");

            migrationBuilder.RenameColumn(
                name: "CausaPrincipal",
                table: "RegistrosMortalidade",
                newName: "Motivo");

            migrationBuilder.AddColumn<string>(
                name: "Setor",
                table: "RegistrosMortalidade",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Setor",
                table: "RegistrosMortalidade");

            migrationBuilder.RenameColumn(
                name: "Quantidade",
                table: "RegistrosMortalidade",
                newName: "QuantidadeMortas");

            migrationBuilder.RenameColumn(
                name: "Motivo",
                table: "RegistrosMortalidade",
                newName: "CausaPrincipal");

            migrationBuilder.AddColumn<string>(
                name: "AcaoTomada",
                table: "RegistrosMortalidade",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AvesVivas",
                table: "RegistrosMortalidade",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "IdadeDias",
                table: "RegistrosMortalidade",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "PesoMedioMortas",
                table: "RegistrosMortalidade",
                type: "numeric",
                nullable: true);
        }
    }
}
