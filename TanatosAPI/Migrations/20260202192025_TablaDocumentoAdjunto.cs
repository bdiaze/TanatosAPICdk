using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TanatosAPI.Migrations
{
    /// <inheritdoc />
    public partial class TablaDocumentoAdjunto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "documento_adjunto",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del documento adjunto.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_historial_norma_suscrita = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del historial de ejecución de una norma suscrita."),
                    bucket_name = table.Column<string>(type: "text", nullable: false, comment: "Nombre del bucket donde está almacenado el documento."),
                    bucket_key = table.Column<string>(type: "text", nullable: false, comment: "Identificador del objeto dentro del bucket."),
                    nombre_archivo = table.Column<string>(type: "text", nullable: false, comment: "Nombre original del archivo."),
                    mime_esperado = table.Column<string>(type: "text", nullable: false, comment: "Mime esperado del archivo."),
                    tamanno_esperado = table.Column<long>(type: "bigint", nullable: false, comment: "Tamaño esperado del archivo en bytes."),
                    mime_real = table.Column<string>(type: "text", nullable: true, comment: "Mime real del archivo."),
                    tamanno_real = table.Column<long>(type: "bigint", nullable: true, comment: "Tamaño real del archivo en bytes."),
                    estado_subida = table.Column<short>(type: "smallint", nullable: false, comment: "Estado de subida del documento adjunto. 0: Generada URL prefirmada para PUT - 1: Documento recepcionado."),
                    fecha_emision_url_prefirmada_put = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se emitió la URL prefirmada para método PUT."),
                    fecha_confirmacion_subida = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se confirmó la subida del archivo."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó el registro."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó el registro."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del registro.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_documento_adjunto", x => x.id);
                    table.ForeignKey(
                        name: "FK_documento_adjunto_historial_norma_suscrita_id_historial_nor~",
                        column: x => x.id_historial_norma_suscrita,
                        principalSchema: "tanatos",
                        principalTable: "historial_norma_suscrita",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene la metadata de los documentos adjuntos asociados al historial de ejecución de una norma suscrita.");

            migrationBuilder.CreateIndex(
                name: "IX_documento_adjunto_bucket_name_bucket_key",
                schema: "tanatos",
                table: "documento_adjunto",
                columns: new[] { "bucket_name", "bucket_key" });

            migrationBuilder.CreateIndex(
                name: "IX_documento_adjunto_id_historial_norma_suscrita",
                schema: "tanatos",
                table: "documento_adjunto",
                column: "id_historial_norma_suscrita");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "documento_adjunto",
                schema: "tanatos");
        }
    }
}
