using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TanatosAPI.Migrations
{
    /// <inheritdoc />
    public partial class TablaNormaSuscrita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "norma_suscrita",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador de la norma suscrita.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sub = table.Column<string>(type: "text", nullable: false, comment: "Usuario al que pertenece la norma suscrita."),
                    id_negocio = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del negocio del usuario."),
                    id_template = table.Column<long>(type: "bigint", nullable: true, comment: "Identificador del template al que pertenece la norma suscrita."),
                    id_norma = table.Column<long>(type: "bigint", nullable: true, comment: "Identificador del template norma al que pertenece la norma suscrita."),
                    nombre = table.Column<string>(type: "text", nullable: true, comment: "Nombre de la norma."),
                    descripcion = table.Column<string>(type: "text", nullable: true, comment: "Descripcion de la norma."),
                    id_tipo_periodicidad = table.Column<long>(type: "bigint", nullable: true, comment: "Identificador del tipo de periodicidad asociado a la norma."),
                    multa = table.Column<string>(type: "text", nullable: true, comment: "Multa de no cumplir con la norma."),
                    id_categoria_norma = table.Column<long>(type: "bigint", nullable: true, comment: "Identificador de la categoría a la que pertenece la norma."),
                    orden_visual = table.Column<long>(type: "bigint", nullable: true, comment: "Orden en que se deben presentar las normas."),
                    editable = table.Column<bool>(type: "boolean", nullable: false, comment: "Indicador de si es editable la norma."),
                    fecha_activacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se activó el cumplimiento de la norma."),
                    fecha_desactivacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se desactivó el cumplimiento de la norma."),
                    activado = table.Column<bool>(type: "boolean", nullable: false, comment: "Estado de activación de la norma."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se creó la norma."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó la norma."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia de la norma.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_norma_suscrita", x => x.id);
                    table.ForeignKey(
                        name: "FK_norma_suscrita_categoria_norma_id_categoria_norma",
                        column: x => x.id_categoria_norma,
                        principalSchema: "tanatos",
                        principalTable: "categoria_norma",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_norma_suscrita_negocio_id_negocio",
                        column: x => x.id_negocio,
                        principalSchema: "tanatos",
                        principalTable: "negocio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_norma_suscrita_template_norma_id_template_id_norma",
                        columns: x => new { x.id_template, x.id_norma },
                        principalSchema: "tanatos",
                        principalTable: "template_norma",
                        principalColumns: new[] { "id_template", "id_norma" },
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_norma_suscrita_tipo_periodicidad_id_tipo_periodicidad",
                        column: x => x.id_tipo_periodicidad,
                        principalSchema: "tanatos",
                        principalTable: "tipo_periodicidad",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene las normas a las que está suscrita un negocio del usuario.");

            migrationBuilder.CreateIndex(
                name: "IX_norma_suscrita_id_categoria_norma",
                schema: "tanatos",
                table: "norma_suscrita",
                column: "id_categoria_norma");

            migrationBuilder.CreateIndex(
                name: "IX_norma_suscrita_id_negocio",
                schema: "tanatos",
                table: "norma_suscrita",
                column: "id_negocio");

            migrationBuilder.CreateIndex(
                name: "IX_norma_suscrita_id_template_id_norma",
                schema: "tanatos",
                table: "norma_suscrita",
                columns: new[] { "id_template", "id_norma" });

            migrationBuilder.CreateIndex(
                name: "IX_norma_suscrita_id_tipo_periodicidad",
                schema: "tanatos",
                table: "norma_suscrita",
                column: "id_tipo_periodicidad");

            migrationBuilder.CreateIndex(
                name: "IX_norma_suscrita_sub_id_negocio",
                schema: "tanatos",
                table: "norma_suscrita",
                columns: new[] { "sub", "id_negocio" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "norma_suscrita",
                schema: "tanatos");
        }
    }
}
