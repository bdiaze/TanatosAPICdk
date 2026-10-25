using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class ColumnIdMovimientoFinancieroDocumentoAdjunto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "id_historial_norma_suscrita",
                schema: "tanatos",
                table: "documento_adjunto",
                type: "bigint",
                nullable: true,
                comment: "Identificador del vencimiento asociado al documento.",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true,
                oldComment: "Identificador del historial de ejecución de una norma suscrita.");

            migrationBuilder.AddColumn<long>(
                name: "id_movimiento_financiero",
                schema: "tanatos",
                table: "documento_adjunto",
                type: "bigint",
                nullable: true,
                comment: "Identificador del movimiento financiero asociado al documento.");

            migrationBuilder.CreateIndex(
                name: "IX_documento_adjunto_id_movimiento_financiero",
                schema: "tanatos",
                table: "documento_adjunto",
                column: "id_movimiento_financiero");

            migrationBuilder.AddForeignKey(
                name: "FK_documento_adjunto_movimiento_financiero_id_movimiento_finan~",
                schema: "tanatos",
                table: "documento_adjunto",
                column: "id_movimiento_financiero",
                principalSchema: "tanatos",
                principalTable: "movimiento_financiero",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_documento_adjunto_movimiento_financiero_id_movimiento_finan~",
                schema: "tanatos",
                table: "documento_adjunto");

            migrationBuilder.DropIndex(
                name: "IX_documento_adjunto_id_movimiento_financiero",
                schema: "tanatos",
                table: "documento_adjunto");

            migrationBuilder.DropColumn(
                name: "id_movimiento_financiero",
                schema: "tanatos",
                table: "documento_adjunto");

            migrationBuilder.AlterColumn<long>(
                name: "id_historial_norma_suscrita",
                schema: "tanatos",
                table: "documento_adjunto",
                type: "bigint",
                nullable: true,
                comment: "Identificador del historial de ejecución de una norma suscrita.",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true,
                oldComment: "Identificador del vencimiento asociado al documento.");
        }
    }
}
