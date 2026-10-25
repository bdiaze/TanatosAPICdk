using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class ColumnIdNegocioDocumentoAdjunto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "id_negocio",
                schema: "tanatos",
                table: "documento_adjunto",
                type: "bigint",
                nullable: true,
                comment: "Identificador del negocio al que pertenece el documento.");

            migrationBuilder.CreateIndex(
                name: "IX_documento_adjunto_id_negocio",
                schema: "tanatos",
                table: "documento_adjunto",
                column: "id_negocio");

            migrationBuilder.AddForeignKey(
                name: "FK_documento_adjunto_negocio_id_negocio",
                schema: "tanatos",
                table: "documento_adjunto",
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
                name: "FK_documento_adjunto_negocio_id_negocio",
                schema: "tanatos",
                table: "documento_adjunto");

            migrationBuilder.DropIndex(
                name: "IX_documento_adjunto_id_negocio",
                schema: "tanatos",
                table: "documento_adjunto");

            migrationBuilder.DropColumn(
                name: "id_negocio",
                schema: "tanatos",
                table: "documento_adjunto");
        }
    }
}
