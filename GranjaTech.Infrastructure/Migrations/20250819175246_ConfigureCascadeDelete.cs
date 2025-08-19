using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GranjaTech.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureCascadeDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransacoesFinanceiras_Lotes_LoteId",
                table: "TransacoesFinanceiras");

            migrationBuilder.AddForeignKey(
                name: "FK_TransacoesFinanceiras_Lotes_LoteId",
                table: "TransacoesFinanceiras",
                column: "LoteId",
                principalTable: "Lotes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TransacoesFinanceiras_Lotes_LoteId",
                table: "TransacoesFinanceiras");

            migrationBuilder.AddForeignKey(
                name: "FK_TransacoesFinanceiras_Lotes_LoteId",
                table: "TransacoesFinanceiras",
                column: "LoteId",
                principalTable: "Lotes",
                principalColumn: "Id");
        }
    }
}
