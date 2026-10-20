using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class ColumnOpcionalCodigoValidacionDestinatarioNotificacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_caducidad_codigo_validacion",
                schema: "tanatos",
                table: "destinatario_notificacion",
                type: "timestamp with time zone",
                nullable: true,
                defaultValueSql: "NOW() + INTERVAL '24 hours'",
                comment: "Fecha en que caduca el código de validación.",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldDefaultValueSql: "NOW() + INTERVAL '24 hours'",
                oldComment: "Fecha en que caduca el código de validación.");

            migrationBuilder.AlterColumn<string>(
                name: "codigo_validacion",
                schema: "tanatos",
                table: "destinatario_notificacion",
                type: "text",
                nullable: true,
                comment: "Código generado para validar que el destinatario se suscribe a la notificación.",
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Código generado para validar que el destinatario se suscribe a la notificación.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_caducidad_codigo_validacion",
                schema: "tanatos",
                table: "destinatario_notificacion",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "NOW() + INTERVAL '24 hours'",
                comment: "Fecha en que caduca el código de validación.",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldDefaultValueSql: "NOW() + INTERVAL '24 hours'",
                oldComment: "Fecha en que caduca el código de validación.");

            migrationBuilder.AlterColumn<string>(
                name: "codigo_validacion",
                schema: "tanatos",
                table: "destinatario_notificacion",
                type: "text",
                nullable: false,
                defaultValue: "",
                comment: "Código generado para validar que el destinatario se suscribe a la notificación.",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Código generado para validar que el destinatario se suscribe a la notificación.");
        }
    }
}
