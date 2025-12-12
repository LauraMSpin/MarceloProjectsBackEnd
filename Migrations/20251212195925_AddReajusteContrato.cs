using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cronograma_atividades_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddReajusteContrato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MesInicioReajuste",
                table: "Contratos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PercentualReajuste",
                table: "Contratos",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MesInicioReajuste",
                table: "Contratos");

            migrationBuilder.DropColumn(
                name: "PercentualReajuste",
                table: "Contratos");
        }
    }
}
