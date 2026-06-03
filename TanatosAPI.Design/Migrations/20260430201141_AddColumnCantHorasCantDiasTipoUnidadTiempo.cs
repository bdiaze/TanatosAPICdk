using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnCantHorasCantDiasTipoUnidadTiempo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "cant_dias",
                schema: "tanatos",
                table: "tipo_unidad_tiempo",
                type: "bigint",
                nullable: true,
                comment: "Cantidad de días que representan a la unidad de tiempo.");

            migrationBuilder.AddColumn<long>(
                name: "cant_horas",
                schema: "tanatos",
                table: "tipo_unidad_tiempo",
                type: "bigint",
                nullable: true,
                comment: "Cantidad de horas que representan a la unidad de tiempo.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cant_dias",
                schema: "tanatos",
                table: "tipo_unidad_tiempo");

            migrationBuilder.DropColumn(
                name: "cant_horas",
                schema: "tanatos",
                table: "tipo_unidad_tiempo");
        }
    }
}
