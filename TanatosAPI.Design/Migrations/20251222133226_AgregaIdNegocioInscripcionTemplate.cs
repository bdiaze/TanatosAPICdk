using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class AgregaIdNegocioInscripcionTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_inscripcion_template",
                schema: "tanatos",
                table: "inscripcion_template");

            migrationBuilder.AddColumn<long>(
                name: "id_negocio",
                schema: "tanatos",
                table: "inscripcion_template",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "Identificador del negocio del usuario.");

            migrationBuilder.AddPrimaryKey(
                name: "PK_inscripcion_template",
                schema: "tanatos",
                table: "inscripcion_template",
                columns: new[] { "sub", "id_negocio", "id_template" });

            migrationBuilder.CreateIndex(
                name: "IX_inscripcion_template_id_negocio",
                schema: "tanatos",
                table: "inscripcion_template",
                column: "id_negocio");

            migrationBuilder.AddForeignKey(
                name: "FK_inscripcion_template_negocio_id_negocio",
                schema: "tanatos",
                table: "inscripcion_template",
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
                name: "FK_inscripcion_template_negocio_id_negocio",
                schema: "tanatos",
                table: "inscripcion_template");

            migrationBuilder.DropPrimaryKey(
                name: "PK_inscripcion_template",
                schema: "tanatos",
                table: "inscripcion_template");

            migrationBuilder.DropIndex(
                name: "IX_inscripcion_template_id_negocio",
                schema: "tanatos",
                table: "inscripcion_template");

            migrationBuilder.DropColumn(
                name: "id_negocio",
                schema: "tanatos",
                table: "inscripcion_template");

            migrationBuilder.AddPrimaryKey(
                name: "PK_inscripcion_template",
                schema: "tanatos",
                table: "inscripcion_template",
                columns: new[] { "sub", "id_template" });
        }
    }
}
