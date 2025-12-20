using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TanatosAPI.Migrations
{
    /// <inheritdoc />
    public partial class TablaHistorialNormaSuscrita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "historial_norma_suscrita",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del historial de ejecución de una norma suscrita.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_norma_suscrita = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador de la norma suscrita."),
                    fecha_vencimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que vencerá la ejecución de la norma suscrita"),
                    fecha_completitud = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se completó la ejecución de la norma suscrita."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó el registro."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó el registro."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del registro.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_historial_norma_suscrita", x => x.id);
                    table.ForeignKey(
                        name: "FK_historial_norma_suscrita_norma_suscrita_id_norma_suscrita",
                        column: x => x.id_norma_suscrita,
                        principalSchema: "tanatos",
                        principalTable: "norma_suscrita",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene el historial de ejecución de una norma suscrita.");

            migrationBuilder.CreateIndex(
                name: "IX_historial_norma_suscrita_fecha_vencimiento",
                schema: "tanatos",
                table: "historial_norma_suscrita",
                column: "fecha_vencimiento");

            migrationBuilder.CreateIndex(
                name: "IX_historial_norma_suscrita_id_norma_suscrita_fecha_vencimiento",
                schema: "tanatos",
                table: "historial_norma_suscrita",
                columns: new[] { "id_norma_suscrita", "fecha_vencimiento" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "historial_norma_suscrita",
                schema: "tanatos");
        }
    }
}
