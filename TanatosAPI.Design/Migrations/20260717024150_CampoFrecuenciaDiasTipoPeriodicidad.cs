using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class CampoFrecuenciaDiasTipoPeriodicidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "frecuencia_dias",
                schema: "tanatos",
                table: "tipo_periodicidad",
                type: "integer",
                nullable: true,
                comment: "Frecuencia en días del tipo de periodicidad.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "frecuencia_dias",
                schema: "tanatos",
                table: "tipo_periodicidad");
        }
    }
}
