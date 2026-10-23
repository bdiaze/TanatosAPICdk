using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class NullableIdHistorialNormaSuscritaDocumentoAdjunto : Migration
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
                comment: "Identificador del historial de ejecución de una norma suscrita.",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "Identificador del historial de ejecución de una norma suscrita.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "id_historial_norma_suscrita",
                schema: "tanatos",
                table: "documento_adjunto",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "Identificador del historial de ejecución de una norma suscrita.",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true,
                oldComment: "Identificador del historial de ejecución de una norma suscrita.");
        }
    }
}
