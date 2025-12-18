using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TanatosAPI.Migrations
{
    /// <inheritdoc />
    public partial class TablaNotificacionesNormaSuscrita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "notificacion_norma_suscrita",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador de la notificación asociada a una norma suscrita.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_norma_suscrita = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador de la norma suscrita."),
                    id_tipo_unidad_tiempo_antelacion = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del tipo de unidad de tiempo a usar para la notificación."),
                    cant_antelacion = table.Column<int>(type: "integer", nullable: false, comment: "Cantidad de unidades de tiempo a usar para la notificación."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se creó la notificación asociada."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó la notificación asociada."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia de la notificación asociada.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notificacion_norma_suscrita", x => x.id);
                    table.ForeignKey(
                        name: "FK_notificacion_norma_suscrita_norma_suscrita_id_norma_suscrita",
                        column: x => x.id_norma_suscrita,
                        principalSchema: "tanatos",
                        principalTable: "norma_suscrita",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_notificacion_norma_suscrita_tipo_unidad_tiempo_id_tipo_unid~",
                        column: x => x.id_tipo_unidad_tiempo_antelacion,
                        principalSchema: "tanatos",
                        principalTable: "tipo_unidad_tiempo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene las notificaciones asociados a una norma suscrita.");

            migrationBuilder.CreateIndex(
                name: "IX_notificacion_norma_suscrita_id_norma_suscrita_vigencia",
                schema: "tanatos",
                table: "notificacion_norma_suscrita",
                columns: new[] { "id_norma_suscrita", "vigencia" });

            migrationBuilder.CreateIndex(
                name: "IX_notificacion_norma_suscrita_id_tipo_unidad_tiempo_antelacion",
                schema: "tanatos",
                table: "notificacion_norma_suscrita",
                column: "id_tipo_unidad_tiempo_antelacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "notificacion_norma_suscrita",
                schema: "tanatos");
        }
    }
}
