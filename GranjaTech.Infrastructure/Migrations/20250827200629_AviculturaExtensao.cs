using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace GranjaTech.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AviculturaExtensao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Identificador",
                table: "Lotes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Lotes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<decimal>(
                name: "AreaGalpao",
                table: "Lotes",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataAbatePrevista",
                table: "Lotes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataAtualizacao",
                table: "Lotes",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataCriacao",
                table: "Lotes",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Linhagem",
                table: "Lotes",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Observacoes",
                table: "Lotes",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrigemPintinhos",
                table: "Lotes",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "QuantidadeAvesAtual",
                table: "Lotes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Lotes",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ConsumosAgua",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LoteId = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    QuantidadeLitros = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    AvesVivas = table.Column<int>(type: "integer", nullable: false),
                    TemperaturaAmbiente = table.Column<decimal>(type: "numeric", nullable: true),
                    Observacoes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumosAgua", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsumosAgua_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsumosRacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LoteId = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    QuantidadeKg = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    TipoRacao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AvesVivas = table.Column<int>(type: "integer", nullable: false),
                    Observacoes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumosRacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConsumosRacao_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EventosSanitarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LoteId = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TipoEvento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Produto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    LoteProduto = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Dosagem = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ViaAdministracao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AvesTratadas = table.Column<int>(type: "integer", nullable: true),
                    DuracaoTratamentoDias = table.Column<int>(type: "integer", nullable: true),
                    PeriodoCarenciaDias = table.Column<int>(type: "integer", nullable: true),
                    ResponsavelAplicacao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Sintomas = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Custo = table.Column<decimal>(type: "numeric", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosSanitarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventosSanitarios_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MedicoesQualidadeAr",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LoteId = table.Column<int>(type: "integer", nullable: false),
                    DataHora = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NH3_ppm = table.Column<decimal>(type: "numeric(6,2)", precision: 6, scale: 2, nullable: true),
                    CO2_ppm = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: true),
                    O2_percentual = table.Column<decimal>(type: "numeric", nullable: true),
                    VelocidadeAr_ms = table.Column<decimal>(type: "numeric", nullable: true),
                    Luminosidade_lux = table.Column<decimal>(type: "numeric", nullable: true),
                    TemperaturaAr = table.Column<decimal>(type: "numeric", nullable: true),
                    UmidadeRelativa = table.Column<decimal>(type: "numeric", nullable: true),
                    LocalMedicao = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EquipamentoMedicao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Observacoes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicoesQualidadeAr", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MedicoesQualidadeAr_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PesagensSemanais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LoteId = table.Column<int>(type: "integer", nullable: false),
                    DataPesagem = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IdadeDias = table.Column<int>(type: "integer", nullable: false),
                    SemanaVida = table.Column<int>(type: "integer", nullable: false),
                    PesoMedioGramas = table.Column<decimal>(type: "numeric(8,2)", precision: 8, scale: 2, nullable: false),
                    QuantidadeAmostrada = table.Column<int>(type: "integer", nullable: false),
                    PesoMinimo = table.Column<decimal>(type: "numeric", nullable: true),
                    PesoMaximo = table.Column<decimal>(type: "numeric", nullable: true),
                    DesvioPadrao = table.Column<decimal>(type: "numeric", nullable: true),
                    CoeficienteVariacao = table.Column<decimal>(type: "numeric", nullable: true),
                    GanhoSemanal = table.Column<decimal>(type: "numeric", nullable: true),
                    Observacoes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PesagensSemanais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PesagensSemanais_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosAbate",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LoteId = table.Column<int>(type: "integer", nullable: false),
                    DataAbate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAbatePrevista = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IdadeAbateDias = table.Column<int>(type: "integer", nullable: false),
                    QuantidadeEnviada = table.Column<int>(type: "integer", nullable: false),
                    PesoVivoTotalKg = table.Column<decimal>(type: "numeric(10,3)", precision: 10, scale: 3, nullable: false),
                    PesoCarcacaTotalKg = table.Column<decimal>(type: "numeric", nullable: true),
                    AvesCondenadas = table.Column<int>(type: "integer", nullable: true),
                    MotivoCondenacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PesoCondenadoKg = table.Column<decimal>(type: "numeric", nullable: true),
                    FrigorificoDestino = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Transportadora = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ValorPorKg = table.Column<decimal>(type: "numeric", nullable: true),
                    ValorTotalRecebido = table.Column<decimal>(type: "numeric", nullable: true),
                    Observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosAbate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrosAbate_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosMortalidade",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    LoteId = table.Column<int>(type: "integer", nullable: false),
                    Data = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    QuantidadeMortas = table.Column<int>(type: "integer", nullable: false),
                    AvesVivas = table.Column<int>(type: "integer", nullable: false),
                    CausaPrincipal = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    IdadeDias = table.Column<int>(type: "integer", nullable: false),
                    PesoMedioMortas = table.Column<decimal>(type: "numeric", nullable: true),
                    Observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AcaoTomada = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ResponsavelRegistro = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosMortalidade", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrosMortalidade_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConsumosAgua_LoteId_Data",
                table: "ConsumosAgua",
                columns: new[] { "LoteId", "Data" });

            migrationBuilder.CreateIndex(
                name: "IX_ConsumosRacao_LoteId_Data",
                table: "ConsumosRacao",
                columns: new[] { "LoteId", "Data" });

            migrationBuilder.CreateIndex(
                name: "IX_EventosSanitarios_LoteId",
                table: "EventosSanitarios",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_MedicoesQualidadeAr_LoteId",
                table: "MedicoesQualidadeAr",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_PesagensSemanais_LoteId_SemanaVida",
                table: "PesagensSemanais",
                columns: new[] { "LoteId", "SemanaVida" });

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosAbate_LoteId",
                table: "RegistrosAbate",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosMortalidade_LoteId_Data",
                table: "RegistrosMortalidade",
                columns: new[] { "LoteId", "Data" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConsumosAgua");

            migrationBuilder.DropTable(
                name: "ConsumosRacao");

            migrationBuilder.DropTable(
                name: "EventosSanitarios");

            migrationBuilder.DropTable(
                name: "MedicoesQualidadeAr");

            migrationBuilder.DropTable(
                name: "PesagensSemanais");

            migrationBuilder.DropTable(
                name: "RegistrosAbate");

            migrationBuilder.DropTable(
                name: "RegistrosMortalidade");

            migrationBuilder.DropColumn(
                name: "AreaGalpao",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "DataAbatePrevista",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "DataAtualizacao",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "DataCriacao",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "Linhagem",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "Observacoes",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "OrigemPintinhos",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "QuantidadeAvesAtual",
                table: "Lotes");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Lotes");

            migrationBuilder.AlterColumn<string>(
                name: "Identificador",
                table: "Lotes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Lotes",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);
        }
    }
}
