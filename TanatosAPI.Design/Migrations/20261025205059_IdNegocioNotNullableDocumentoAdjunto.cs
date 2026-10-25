using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class IdNegocioNotNullableDocumentoAdjunto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "id_negocio",
                schema: "tanatos",
                table: "documento_adjunto",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "Identificador del negocio al que pertenece el documento.",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true,
                oldComment: "Identificador del negocio al que pertenece el documento.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "id_negocio",
                schema: "tanatos",
                table: "documento_adjunto",
                type: "bigint",
                nullable: true,
                comment: "Identificador del negocio al que pertenece el documento.",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "Identificador del negocio al que pertenece el documento.");
        }
    }
}
