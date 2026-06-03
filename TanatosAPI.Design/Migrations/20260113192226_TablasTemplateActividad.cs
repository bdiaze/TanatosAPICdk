using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class TablasTemplateActividad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "template_actividad",
                schema: "tanatos",
                columns: table => new
                {
                    id_template = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del template."),
                    id_tipo_actividad = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del tipo de actividad del negocio.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_template_actividad", x => new { x.id_template, x.id_tipo_actividad });
                    table.ForeignKey(
                        name: "FK_template_actividad_template_id_template",
                        column: x => x.id_template,
                        principalSchema: "tanatos",
                        principalTable: "template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_template_actividad_tipo_actividad_id_tipo_actividad",
                        column: x => x.id_tipo_actividad,
                        principalSchema: "tanatos",
                        principalTable: "tipo_actividad",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene la recomendación de templates según tipo de actividad de un negocio.");

            migrationBuilder.CreateIndex(
                name: "IX_template_actividad_id_tipo_actividad",
                schema: "tanatos",
                table: "template_actividad",
                column: "id_tipo_actividad");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "template_actividad",
                schema: "tanatos");
        }
    }
}
