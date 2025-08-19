using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GranjaTech.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddManyToManyFinanceiroProdutor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProdutorOwnerId",
                table: "Usuarios");

            migrationBuilder.CreateTable(
                name: "FinanceiroProdutor",
                columns: table => new
                {
                    FinanceiroId = table.Column<int>(type: "integer", nullable: false),
                    ProdutorId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinanceiroProdutor", x => new { x.FinanceiroId, x.ProdutorId });
                    table.ForeignKey(
                        name: "FK_FinanceiroProdutor_Usuarios_FinanceiroId",
                        column: x => x.FinanceiroId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinanceiroProdutor_Usuarios_ProdutorId",
                        column: x => x.ProdutorId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinanceiroProdutor_ProdutorId",
                table: "FinanceiroProdutor",
                column: "ProdutorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinanceiroProdutor");

            migrationBuilder.AddColumn<int>(
                name: "ProdutorOwnerId",
                table: "Usuarios",
                type: "integer",
                nullable: true);
        }
    }
}
