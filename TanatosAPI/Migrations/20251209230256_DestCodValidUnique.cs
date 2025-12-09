using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Migrations
{
    /// <inheritdoc />
    public partial class DestCodValidUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "intentos_validacion",
                schema: "tanatos",
                table: "destinatario_notificacion");

            migrationBuilder.CreateIndex(
                name: "IX_destinatario_notificacion_codigo_validacion",
                schema: "tanatos",
                table: "destinatario_notificacion",
                column: "codigo_validacion",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_destinatario_notificacion_codigo_validacion",
                schema: "tanatos",
                table: "destinatario_notificacion");

            migrationBuilder.AddColumn<short>(
                name: "intentos_validacion",
                schema: "tanatos",
                table: "destinatario_notificacion",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0,
                comment: "Cantidad de intentos de validar al destinatario.");
        }
    }
}
