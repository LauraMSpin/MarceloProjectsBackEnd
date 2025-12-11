using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace cronograma_atividades_backend.Migrations
{
    /// <inheritdoc />
    public partial class AddOrdemToMedicao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Ordem",
                table: "Medicoes",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ordem",
                table: "Medicoes");
        }
    }
}
