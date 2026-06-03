using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class NombrePluralUnidadTiempo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "nombre_plural",
                schema: "tanatos",
                table: "tipo_unidad_tiempo",
                type: "text",
                nullable: true,
                comment: "Nombre plural del tipo de unidad de tiempo.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "nombre_plural",
                schema: "tanatos",
                table: "tipo_unidad_tiempo");
        }
    }
}
