using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class ColumnCodigoAccesoHistorialNotificacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "codigo_acceso",
                schema: "tanatos",
                table: "historial_notificacion",
                type: "text",
                nullable: true,
                comment: "Código generado para acceder al vencimiento desde la notificación.");

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_caducidad_codigo_acceso",
                schema: "tanatos",
                table: "historial_notificacion",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Fecha en que caduca el código de acceso.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "codigo_acceso",
                schema: "tanatos",
                table: "historial_notificacion");

            migrationBuilder.DropColumn(
                name: "fecha_caducidad_codigo_acceso",
                schema: "tanatos",
                table: "historial_notificacion");
        }
    }
}
