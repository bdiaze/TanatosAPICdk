using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIdentityTipoProcesoAutomatico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_tipo_proceso_automatico_nombre",
                schema: "tanatos",
                table: "tipo_proceso_automatico");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "tanatos",
                table: "tipo_proceso_automatico",
                type: "bigint",
                nullable: false,
                comment: "Identificador del tipo de proceso automático.",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "Identificador del tipo de proceso automático.")
                .OldAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "tanatos",
                table: "tipo_proceso_automatico",
                type: "bigint",
                nullable: false,
                comment: "Identificador del tipo de proceso automático.",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "Identificador del tipo de proceso automático.")
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.CreateIndex(
                name: "IX_tipo_proceso_automatico_nombre",
                schema: "tanatos",
                table: "tipo_proceso_automatico",
                column: "nombre");
        }
    }
}
