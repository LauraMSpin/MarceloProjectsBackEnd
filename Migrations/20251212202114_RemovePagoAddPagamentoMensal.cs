using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cronograma_atividades_backend.Migrations
{
    /// <inheritdoc />
    public partial class RemovePagoAddPagamentoMensal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Pago",
                table: "Medicoes");

            migrationBuilder.CreateTable(
                name: "PagamentosMensais",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Ordem = table.Column<int>(type: "integer", nullable: false),
                    Mes = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ContratoId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagamentosMensais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagamentosMensais_Contratos_ContratoId",
                        column: x => x.ContratoId,
                        principalTable: "Contratos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PagamentosMensais_ContratoId_Ordem",
                table: "PagamentosMensais",
                columns: new[] { "ContratoId", "Ordem" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PagamentosMensais");

            migrationBuilder.AddColumn<decimal>(
                name: "Pago",
                table: "Medicoes",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }
    }
}
