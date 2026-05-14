using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Migrations
{
    /// <inheritdoc />
    public partial class ColumnIdEmpleadoDestinatarioNotificacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_destinatario_notificacion_sub_id_negocio_id_tipo_receptor",
                schema: "tanatos",
                table: "destinatario_notificacion");

            migrationBuilder.AddColumn<long>(
                name: "id_empleado",
                schema: "tanatos",
                table: "destinatario_notificacion",
                type: "bigint",
                nullable: true,
                comment: "Identificador del empleado al que pertenece el destino.");

            migrationBuilder.CreateIndex(
                name: "IX_destinatario_notificacion_id_empleado",
                schema: "tanatos",
                table: "destinatario_notificacion",
                column: "id_empleado");

            migrationBuilder.CreateIndex(
                name: "IX_destinatario_notificacion_sub_id_negocio_id_empleado",
                schema: "tanatos",
                table: "destinatario_notificacion",
                columns: new[] { "sub", "id_negocio", "id_empleado" });

            migrationBuilder.AddForeignKey(
                name: "FK_destinatario_notificacion_empleado_id_empleado",
                schema: "tanatos",
                table: "destinatario_notificacion",
                column: "id_empleado",
                principalSchema: "tanatos",
                principalTable: "empleado",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_destinatario_notificacion_empleado_id_empleado",
                schema: "tanatos",
                table: "destinatario_notificacion");

            migrationBuilder.DropIndex(
                name: "IX_destinatario_notificacion_id_empleado",
                schema: "tanatos",
                table: "destinatario_notificacion");

            migrationBuilder.DropIndex(
                name: "IX_destinatario_notificacion_sub_id_negocio_id_empleado",
                schema: "tanatos",
                table: "destinatario_notificacion");

            migrationBuilder.DropColumn(
                name: "id_empleado",
                schema: "tanatos",
                table: "destinatario_notificacion");

            migrationBuilder.CreateIndex(
                name: "IX_destinatario_notificacion_sub_id_negocio_id_tipo_receptor",
                schema: "tanatos",
                table: "destinatario_notificacion",
                columns: new[] { "sub", "id_negocio", "id_tipo_receptor" });
        }
    }
}
