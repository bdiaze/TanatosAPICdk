using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class RenameColumnHistorialNotificacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "hermes_queue_message_id",
                schema: "tanatos",
                table: "historial_notificacion",
                newName: "hermes_id_mensaje");

			migrationBuilder.AlterColumn<string>(
				name: "hermes_id_mensaje",
				schema: "tanatos",
				table: "historial_notificacion",
				type: "text",
				nullable: true,
				comment: "ID del mensaje en Hermes.",
				oldClrType: typeof(string),
				oldType: "text",
				oldNullable: true,
				oldComment: "ID del mensaje en la cola de envío de correo de Hermes.");
		}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
			migrationBuilder.RenameColumn(
				name: "hermes_id_mensaje",
				schema: "tanatos",
				table: "historial_notificacion",
				newName: "hermes_queue_message_id");

			migrationBuilder.AlterColumn<string>(
				name: "hermes_queue_message_id",
				schema: "tanatos",
				table: "historial_notificacion",
				type: "text",
				nullable: true,
				comment: "ID del mensaje en la cola de envío de correo de Hermes.",
				oldClrType: typeof(string),
				oldType: "text",
				oldNullable: true,
				oldComment: "ID del mensaje en Hermes.");
		}
    }
}
