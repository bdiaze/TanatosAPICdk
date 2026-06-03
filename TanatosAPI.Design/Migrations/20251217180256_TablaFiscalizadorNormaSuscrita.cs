using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class TablaFiscalizadorNormaSuscrita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "fiscalizador_norma_suscrita",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del fiscalizador asociado a una norma suscrita.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_norma_suscrita = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador de la norma suscrita."),
                    id_tipo_fiscalizador = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del fiscalizador asociado."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se creó al fiscalizador asociado."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó al fiscalizador asociado."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del fiscalizador asociado.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fiscalizador_norma_suscrita", x => x.id);
                    table.ForeignKey(
                        name: "FK_fiscalizador_norma_suscrita_norma_suscrita_id_norma_suscrita",
                        column: x => x.id_norma_suscrita,
                        principalSchema: "tanatos",
                        principalTable: "norma_suscrita",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_fiscalizador_norma_suscrita_tipo_fiscalizador_id_tipo_fisca~",
                        column: x => x.id_tipo_fiscalizador,
                        principalSchema: "tanatos",
                        principalTable: "tipo_fiscalizador",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene los fiscalizadores asociados a una norma suscrita.");

            migrationBuilder.CreateIndex(
                name: "IX_fiscalizador_norma_suscrita_id_norma_suscrita_vigencia",
                schema: "tanatos",
                table: "fiscalizador_norma_suscrita",
                columns: new[] { "id_norma_suscrita", "vigencia" });

            migrationBuilder.CreateIndex(
                name: "IX_fiscalizador_norma_suscrita_id_tipo_fiscalizador",
                schema: "tanatos",
                table: "fiscalizador_norma_suscrita",
                column: "id_tipo_fiscalizador");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fiscalizador_norma_suscrita",
                schema: "tanatos");
        }
    }
}
