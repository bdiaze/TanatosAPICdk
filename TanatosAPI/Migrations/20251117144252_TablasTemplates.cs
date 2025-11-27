using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Migrations
{
    /// <inheritdoc />
    public partial class TablasTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "template",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del template."),
                    id_template_padre = table.Column<long>(type: "bigint", nullable: true, comment: "Identificador del template padre."),
                    nombre = table.Column<string>(type: "text", nullable: false, comment: "Nombre del template."),
                    descripcion = table.Column<string>(type: "text", nullable: false, comment: "Descripcion del template."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del template.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_template", x => x.id);
                    table.ForeignKey(
                        name: "FK_template_template_id_template_padre",
                        column: x => x.id_template_padre,
                        principalSchema: "tanatos",
                        principalTable: "template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene los templates de normas a inscribirse.");

            migrationBuilder.CreateTable(
                name: "inscripcion_template",
                schema: "tanatos",
                columns: table => new
                {
                    sub = table.Column<string>(type: "text", nullable: false, comment: "Usuario al que está asociada la inscripción."),
                    id_template = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del template al que está inscrito el usuario."),
                    fecha_activacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se activa la inscripción."),
                    fecha_desactivacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se desactiva la inscripción."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia de la inscripción.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_inscripcion_template", x => new { x.sub, x.id_template });
                    table.ForeignKey(
                        name: "FK_inscripcion_template_template_id_template",
                        column: x => x.id_template,
                        principalSchema: "tanatos",
                        principalTable: "template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene los templates a los que un usuario está inscrito.");

            migrationBuilder.CreateTable(
                name: "template_norma",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador de la norma asociada al template."),
                    id_template = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del template al que pertenece la norma."),
                    nombre = table.Column<string>(type: "text", nullable: false, comment: "Nombre de la norma."),
                    descripcion = table.Column<string>(type: "text", nullable: true, comment: "Descripcion de la norma."),
                    id_tipo_periodicidad = table.Column<long>(type: "bigint", nullable: true, comment: "Identificador del tipo de periodicidad asociado a la norma."),
                    multa = table.Column<string>(type: "text", nullable: true, comment: "Multa de no cumplir con la norma"),
                    id_categoria_norma = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador de la categoría a la que pertenece la norma.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_template_norma", x => x.id);
                    table.ForeignKey(
                        name: "FK_template_norma_categoria_norma_id_categoria_norma",
                        column: x => x.id_categoria_norma,
                        principalSchema: "tanatos",
                        principalTable: "categoria_norma",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_template_norma_template_id_template",
                        column: x => x.id_template,
                        principalSchema: "tanatos",
                        principalTable: "template",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_template_norma_tipo_periodicidad_id_tipo_periodicidad",
                        column: x => x.id_tipo_periodicidad,
                        principalSchema: "tanatos",
                        principalTable: "tipo_periodicidad",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene las normas asociadas a un template.");

            migrationBuilder.CreateTable(
                name: "template_norma_fiscalizador",
                schema: "tanatos",
                columns: table => new
                {
                    id_template_norma = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador de la norma perteneciente a un template."),
                    id_tipo_fiscalizador = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del tipo de fiscalizador.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_template_norma_fiscalizador", x => new { x.id_template_norma, x.id_tipo_fiscalizador });
                    table.ForeignKey(
                        name: "FK_template_norma_fiscalizador_template_norma_id_template_norma",
                        column: x => x.id_template_norma,
                        principalSchema: "tanatos",
                        principalTable: "template_norma",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_template_norma_fiscalizador_tipo_fiscalizador_id_tipo_fisca~",
                        column: x => x.id_tipo_fiscalizador,
                        principalSchema: "tanatos",
                        principalTable: "tipo_fiscalizador",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene la relación entre un template norma y un fiscalizador.");

            migrationBuilder.CreateTable(
                name: "template_norma_notificacion",
                schema: "tanatos",
                columns: table => new
                {
                    id_template_norma = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador de la template norma."),
                    id_tipo_unidad_tiempo_antelacion = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del tipo de unidad de tiempo a usar para la notificación."),
                    cant_antelacion = table.Column<int>(type: "integer", nullable: false, comment: "Cantidad de unidades de tiempo a usar para la notificación.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_template_norma_notificacion", x => new { x.id_template_norma, x.id_tipo_unidad_tiempo_antelacion });
                    table.ForeignKey(
                        name: "FK_template_norma_notificacion_template_norma_id_template_norma",
                        column: x => x.id_template_norma,
                        principalSchema: "tanatos",
                        principalTable: "template_norma",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_template_norma_notificacion_tipo_unidad_tiempo_id_tipo_unid~",
                        column: x => x.id_tipo_unidad_tiempo_antelacion,
                        principalSchema: "tanatos",
                        principalTable: "tipo_unidad_tiempo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene las notificaciones asociadas a una template norma.");

            migrationBuilder.CreateIndex(
                name: "IX_inscripcion_template_id_template",
                schema: "tanatos",
                table: "inscripcion_template",
                column: "id_template");

            migrationBuilder.CreateIndex(
                name: "IX_template_id_template_padre",
                schema: "tanatos",
                table: "template",
                column: "id_template_padre");

            migrationBuilder.CreateIndex(
                name: "IX_template_norma_id_categoria_norma",
                schema: "tanatos",
                table: "template_norma",
                column: "id_categoria_norma");

            migrationBuilder.CreateIndex(
                name: "IX_template_norma_id_template",
                schema: "tanatos",
                table: "template_norma",
                column: "id_template");

            migrationBuilder.CreateIndex(
                name: "IX_template_norma_id_tipo_periodicidad",
                schema: "tanatos",
                table: "template_norma",
                column: "id_tipo_periodicidad");

            migrationBuilder.CreateIndex(
                name: "IX_template_norma_fiscalizador_id_tipo_fiscalizador",
                schema: "tanatos",
                table: "template_norma_fiscalizador",
                column: "id_tipo_fiscalizador");

            migrationBuilder.CreateIndex(
                name: "IX_template_norma_notificacion_id_tipo_unidad_tiempo_antelacion",
                schema: "tanatos",
                table: "template_norma_notificacion",
                column: "id_tipo_unidad_tiempo_antelacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "inscripcion_template",
                schema: "tanatos");

            migrationBuilder.DropTable(
                name: "template_norma_fiscalizador",
                schema: "tanatos");

            migrationBuilder.DropTable(
                name: "template_norma_notificacion",
                schema: "tanatos");

            migrationBuilder.DropTable(
                name: "template_norma",
                schema: "tanatos");

            migrationBuilder.DropTable(
                name: "template",
                schema: "tanatos");
        }
    }
}
