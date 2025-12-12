using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cronograma_atividades_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddContratoCompartilhado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContratosCompartilhados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ContratoId = table.Column<Guid>(type: "uuid", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    DataCompartilhamento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PodeEditar = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContratosCompartilhados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContratosCompartilhados_Contratos_ContratoId",
                        column: x => x.ContratoId,
                        principalTable: "Contratos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContratosCompartilhados_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContratosCompartilhados_ContratoId_UsuarioId",
                table: "ContratosCompartilhados",
                columns: new[] { "ContratoId", "UsuarioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContratosCompartilhados_UsuarioId",
                table: "ContratosCompartilhados",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContratosCompartilhados");
        }
    }
}
