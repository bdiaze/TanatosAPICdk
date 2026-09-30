using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class ColumnsFechaNotificacionHermesIdMensajeSuscripcion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_notificacion",
                schema: "tanatos",
                table: "suscripcion",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Fecha en que se notifica contratación del plan.");

            migrationBuilder.AddColumn<string>(
                name: "hermes_id_mensaje",
                schema: "tanatos",
                table: "suscripcion",
                type: "text",
                nullable: true,
                comment: "ID del mensaje en Hermes asociado a la notificación de contratación.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fecha_notificacion",
                schema: "tanatos",
                table: "suscripcion");

            migrationBuilder.DropColumn(
                name: "hermes_id_mensaje",
                schema: "tanatos",
                table: "suscripcion");
        }
    }
}
