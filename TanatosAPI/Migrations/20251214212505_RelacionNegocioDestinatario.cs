using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Migrations
{
    /// <inheritdoc />
    public partial class RelacionNegocioDestinatario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_destinatario_notificacion_sub_id_tipo_receptor",
                schema: "tanatos",
                table: "destinatario_notificacion");

            migrationBuilder.AddColumn<long>(
                name: "id_negocio",
                schema: "tanatos",
                table: "destinatario_notificacion",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "Identificador del negocio del usuario.");

            migrationBuilder.CreateIndex(
                name: "IX_destinatario_notificacion_id_negocio",
                schema: "tanatos",
                table: "destinatario_notificacion",
                column: "id_negocio");

            migrationBuilder.CreateIndex(
                name: "IX_destinatario_notificacion_sub_id_negocio_id_tipo_receptor",
                schema: "tanatos",
                table: "destinatario_notificacion",
                columns: new[] { "sub", "id_negocio", "id_tipo_receptor" });

            migrationBuilder.AddForeignKey(
                name: "FK_destinatario_notificacion_negocio_id_negocio",
                schema: "tanatos",
                table: "destinatario_notificacion",
                column: "id_negocio",
                principalSchema: "tanatos",
                principalTable: "negocio",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_destinatario_notificacion_negocio_id_negocio",
                schema: "tanatos",
                table: "destinatario_notificacion");

            migrationBuilder.DropIndex(
                name: "IX_destinatario_notificacion_id_negocio",
                schema: "tanatos",
                table: "destinatario_notificacion");

            migrationBuilder.DropIndex(
                name: "IX_destinatario_notificacion_sub_id_negocio_id_tipo_receptor",
                schema: "tanatos",
                table: "destinatario_notificacion");

            migrationBuilder.DropColumn(
                name: "id_negocio",
                schema: "tanatos",
                table: "destinatario_notificacion");

            migrationBuilder.CreateIndex(
                name: "IX_destinatario_notificacion_sub_id_tipo_receptor",
                schema: "tanatos",
                table: "destinatario_notificacion",
                columns: new[] { "sub", "id_tipo_receptor" });
        }
    }
}
