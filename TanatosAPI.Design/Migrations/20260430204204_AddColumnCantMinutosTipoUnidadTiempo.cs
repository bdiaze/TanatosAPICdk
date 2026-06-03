using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnCantMinutosTipoUnidadTiempo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "cant_minutos",
                schema: "tanatos",
                table: "tipo_unidad_tiempo",
                type: "bigint",
                nullable: true,
                comment: "Cantidad de minutos que representan a la unidad de tiempo.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cant_minutos",
                schema: "tanatos",
                table: "tipo_unidad_tiempo");
        }
    }
}
