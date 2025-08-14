using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GranjaTech.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Granjas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Localizacao = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Granjas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Identificador = table.Column<string>(type: "text", nullable: false),
                    DataEntrada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataSaida = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QuantidadeAvesInicial = table.Column<int>(type: "integer", nullable: false),
                    GranjaId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lotes_Granjas_GranjaId",
                        column: x => x.GranjaId,
                        principalTable: "Granjas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_GranjaId",
                table: "Lotes",
                column: "GranjaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lotes");

            migrationBuilder.DropTable(
                name: "Granjas");
        }
    }
}
