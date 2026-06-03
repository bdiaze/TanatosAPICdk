using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class ColumnFechaInicioExpiracionSuscripcion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_inicio",
                schema: "tanatos",
                table: "suscripcion",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Fecha en que se inició la suscripción.",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "Fecha en que se inició la suscripción.");

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_expiracion",
                schema: "tanatos",
                table: "suscripcion",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Fecha en que expira la suscripción.",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "Fecha en que expira la suscripción.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_inicio",
                schema: "tanatos",
                table: "suscripcion",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                comment: "Fecha en que se inició la suscripción.",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Fecha en que se inició la suscripción.");

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_expiracion",
                schema: "tanatos",
                table: "suscripcion",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                comment: "Fecha en que expira la suscripción.",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Fecha en que expira la suscripción.");
        }
    }
}
