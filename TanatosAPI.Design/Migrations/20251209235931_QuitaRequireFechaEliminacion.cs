using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class QuitaRequireFechaEliminacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "fecha_eliminacion",
                schema: "tanatos",
                table: "destinatario_notificacion",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Fecha en que se eliminó el destinatario.",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldComment: "Fecha en que se eliminó el destinatario.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "fecha_eliminacion",
                schema: "tanatos",
                table: "destinatario_notificacion",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                comment: "Fecha en que se eliminó el destinatario.",
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Fecha en que se eliminó el destinatario.");
        }
    }
}
