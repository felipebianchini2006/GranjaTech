using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GranjaTech.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserOwnershipToEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProdutorOwnerId",
                table: "Usuarios",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioId",
                table: "Granjas",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Granjas_UsuarioId",
                table: "Granjas",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Granjas_Usuarios_UsuarioId",
                table: "Granjas",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Granjas_Usuarios_UsuarioId",
                table: "Granjas");

            migrationBuilder.DropIndex(
                name: "IX_Granjas_UsuarioId",
                table: "Granjas");

            migrationBuilder.DropColumn(
                name: "ProdutorOwnerId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Granjas");
        }
    }
}
