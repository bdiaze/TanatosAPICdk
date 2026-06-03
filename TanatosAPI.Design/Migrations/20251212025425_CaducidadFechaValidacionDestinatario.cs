using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class CaducidadFechaValidacionDestinatario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_caducidad_codigo_validacion",
                schema: "tanatos",
                table: "destinatario_notificacion",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW() + INTERVAL '24 hours'",
                comment: "Fecha en que caduca el código de validación.");

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_validacion",
                schema: "tanatos",
                table: "destinatario_notificacion",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Fecha en que se validó el destinatario.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fecha_caducidad_codigo_validacion",
                schema: "tanatos",
                table: "destinatario_notificacion");

            migrationBuilder.DropColumn(
                name: "fecha_validacion",
                schema: "tanatos",
                table: "destinatario_notificacion");
        }
    }
}
