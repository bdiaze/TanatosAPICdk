using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Migrations
{
    /// <inheritdoc />
    public partial class ColumnsHistorialNotificacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "cant_antelacion",
                schema: "tanatos",
                table: "historial_notificacion",
                type: "integer",
                nullable: true,
                comment: "Cantidad de unidades de tiempo correspondientes a la notificación.");

            migrationBuilder.AddColumn<long>(
                name: "id_tipo_unidad_tiempo_antelacion",
                schema: "tanatos",
                table: "historial_notificacion",
                type: "bigint",
                nullable: true,
                comment: "Identificador del tipo de unidad de tiempo correspondiente a la notificación.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cant_antelacion",
                schema: "tanatos",
                table: "historial_notificacion");

            migrationBuilder.DropColumn(
                name: "id_tipo_unidad_tiempo_antelacion",
                schema: "tanatos",
                table: "historial_notificacion");
        }
    }
}
