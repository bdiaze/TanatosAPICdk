using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Migrations
{
    /// <inheritdoc />
    public partial class ColumnsMetadataHistorialNotificacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_creacion",
                schema: "tanatos",
                table: "historial_notificacion",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                comment: "Fecha en que se creó el registro.");

            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_eliminacion",
                schema: "tanatos",
                table: "historial_notificacion",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Fecha en que se eliminó el registro.");

            migrationBuilder.AddColumn<bool>(
                name: "vigencia",
                schema: "tanatos",
                table: "historial_notificacion",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Vigencia del registro.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fecha_creacion",
                schema: "tanatos",
                table: "historial_notificacion");

            migrationBuilder.DropColumn(
                name: "fecha_eliminacion",
                schema: "tanatos",
                table: "historial_notificacion");

            migrationBuilder.DropColumn(
                name: "vigencia",
                schema: "tanatos",
                table: "historial_notificacion");
        }
    }
}
