using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TanatosAPI.Migrations
{
    /// <inheritdoc />
    public partial class TablaHistorialNotificacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "historial_notificacion",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del historial de notificación de una norma suscrita.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_historial_norma_suscrita = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del historial de ejecución de una norma suscrita."),
                    id_destinatario_notificacion = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del destinatario de la notificación."),
                    fecha_programacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se programó el envío de la notificación."),
                    fecha_ejecucion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se ejecutó el envío de la notificación.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historial_notificacion", x => x.id);
                    table.ForeignKey(
                        name: "FK_historial_notificacion_destinatario_notificacion_id_destina~",
                        column: x => x.id_destinatario_notificacion,
                        principalSchema: "tanatos",
                        principalTable: "destinatario_notificacion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_historial_notificacion_historial_norma_suscrita_id_historia~",
                        column: x => x.id_historial_norma_suscrita,
                        principalSchema: "tanatos",
                        principalTable: "historial_norma_suscrita",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene el historial de notificaciones de una norma suscrita.");

            migrationBuilder.CreateIndex(
                name: "IX_historial_notificacion_id_destinatario_notificacion",
                schema: "tanatos",
                table: "historial_notificacion",
                column: "id_destinatario_notificacion");

            migrationBuilder.CreateIndex(
                name: "IX_historial_notificacion_id_historial_norma_suscrita",
                schema: "tanatos",
                table: "historial_notificacion",
                column: "id_historial_norma_suscrita");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "historial_notificacion",
                schema: "tanatos");
        }
    }
}
