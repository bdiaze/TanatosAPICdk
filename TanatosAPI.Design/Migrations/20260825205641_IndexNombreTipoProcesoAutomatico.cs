using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class IndexNombreTipoProcesoAutomatico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_tipo_proceso_automatico_nombre",
                schema: "tanatos",
                table: "tipo_proceso_automatico",
                column: "nombre");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tipo_proceso_automatico_nombre",
                schema: "tanatos",
                table: "tipo_proceso_automatico");
        }
    }
}
