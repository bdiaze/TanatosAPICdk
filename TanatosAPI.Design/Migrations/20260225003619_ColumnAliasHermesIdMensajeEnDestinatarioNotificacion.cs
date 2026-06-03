using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class ColumnAliasHermesIdMensajeEnDestinatarioNotificacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "alias",
                schema: "tanatos",
                table: "destinatario_notificacion",
                type: "text",
                nullable: true,
                comment: "Alias del destinatario.");

            migrationBuilder.AddColumn<string>(
                name: "hermes_id_mensaje",
                schema: "tanatos",
                table: "destinatario_notificacion",
                type: "text",
                nullable: true,
                comment: "ID del mensaje en Hermes.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "alias",
                schema: "tanatos",
                table: "destinatario_notificacion");

            migrationBuilder.DropColumn(
                name: "hermes_id_mensaje",
                schema: "tanatos",
                table: "destinatario_notificacion");
        }
    }
}
