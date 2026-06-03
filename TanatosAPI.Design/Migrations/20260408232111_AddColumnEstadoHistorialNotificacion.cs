using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnEstadoHistorialNotificacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "estado",
                schema: "tanatos",
                table: "historial_notificacion",
                type: "smallint",
                nullable: true,
                comment: "Estado de la notificación - 0: Pendiente - 1: Enviado - 2: Omitido.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "estado",
                schema: "tanatos",
                table: "historial_notificacion");
        }
    }
}
