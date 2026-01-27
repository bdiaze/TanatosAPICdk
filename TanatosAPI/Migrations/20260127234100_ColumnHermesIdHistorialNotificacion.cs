using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Migrations
{
    /// <inheritdoc />
    public partial class ColumnHermesIdHistorialNotificacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "hermes_queue_message_id",
                schema: "tanatos",
                table: "historial_notificacion",
                type: "text",
                nullable: true,
                comment: "ID del mensaje en la cola de envío de correo de Hermes.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "hermes_queue_message_id",
                schema: "tanatos",
                table: "historial_notificacion");
        }
    }
}
