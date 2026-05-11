using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Migrations
{
    /// <inheritdoc />
    public partial class IndexCodigoAccesoHistorialNotificacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_historial_notificacion_codigo_acceso",
                schema: "tanatos",
                table: "historial_notificacion",
                column: "codigo_acceso",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_historial_notificacion_codigo_acceso",
                schema: "tanatos",
                table: "historial_notificacion");
        }
    }
}
