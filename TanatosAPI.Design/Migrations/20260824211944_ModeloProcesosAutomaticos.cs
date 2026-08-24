using System;
using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using TanatosAPI.Entities.Models;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class ModeloProcesosAutomaticos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<List<ProcesoNotificacion>>(
                name: "procesos_notificaciones",
                schema: "tanatos",
                table: "norma_suscrita",
                type: "jsonb",
                nullable: false,
                comment: "Procesos de notificaciones asociados a la norma suscrita.",
                oldClrType: typeof(List<Dictionary<string, JsonElement>>),
                oldType: "jsonb",
                oldNullable: true,
                oldComment: "Procesos de notificaciones asociados a la norma suscrita.");

            migrationBuilder.CreateTable(
                name: "tipo_proceso_automatico",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del tipo de proceso automático.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false, comment: "Nombre del tipo de proceso automático."),
                    descripcion = table.Column<string>(type: "text", nullable: true, comment: "Descripción del tipo de proceso automático."),
                    habilitado = table.Column<bool>(type: "boolean", nullable: false, comment: "Indicador de si el tipo de proceso automático está habilitado."),
                    orden = table.Column<int>(type: "integer", nullable: false, comment: "Orden en que se presenta el tipo de proceso automático."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó el tipo de proceso automático."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó el tipo de proceso automático."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del tipo de proceso automático.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipo_proceso_automatico", x => x.id);
                },
                comment: "Tabla que contiene los tipos de procesos automáticos.");

            migrationBuilder.CreateTable(
                name: "proceso_automatico",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del proceso automático.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_tipo_proceso_automatico = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del tipo de proceso automático."),
                    id_proceso_kairos = table.Column<string>(type: "text", nullable: false, comment: "Identificador del proceso en Kairos."),
                    id_calendarizacion_kairos = table.Column<string>(type: "text", nullable: false, comment: "Identificador de la calendarización en Kairos."),
                    nombre = table.Column<string>(type: "text", nullable: false, comment: "Nombre del proceso automático."),
                    arn_rol = table.Column<string>(type: "text", nullable: false, comment: "ARN del rol con permisos de ejecución del proceso automático."),
                    arn_proceso = table.Column<string>(type: "text", nullable: false, comment: "ARN del proceso automático."),
                    parametros = table.Column<string>(type: "text", nullable: false, comment: "Parámetros requeridos por el proceso automático."),
                    cron = table.Column<string>(type: "text", nullable: true, comment: "Cron de ejecución del proceso automático."),
                    frecuencia_dias = table.Column<int>(type: "integer", nullable: true, comment: "Frecuencia en días en que se ejecuta el proceso automático."),
                    inicio_ejecucion_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que inicia las ejecuciones del proceso automático."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó el proceso automático."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó el proceso automático."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del proceso automático.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_proceso_automatico", x => x.id);
                    table.ForeignKey(
                        name: "FK_proceso_automatico_tipo_proceso_automatico_id_tipo_proceso_~",
                        column: x => x.id_tipo_proceso_automatico,
                        principalSchema: "tanatos",
                        principalTable: "tipo_proceso_automatico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene los procesos automáticos.");

            migrationBuilder.CreateTable(
                name: "norma_suscrita_proceso_notificacion",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador de la relación.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_norma_suscrita = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador de la norma suscrita."),
                    id_proceso_automatico = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del proceso de notificación."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó la relación."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó la relación."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia de la relación.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_norma_suscrita_proceso_notificacion", x => x.id);
                    table.ForeignKey(
                        name: "FK_norma_suscrita_proceso_notificacion_norma_suscrita_id_norma~",
                        column: x => x.id_norma_suscrita,
                        principalSchema: "tanatos",
                        principalTable: "norma_suscrita",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_norma_suscrita_proceso_notificacion_proceso_automatico_id_p~",
                        column: x => x.id_proceso_automatico,
                        principalSchema: "tanatos",
                        principalTable: "proceso_automatico",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene las relaciones entre normas suscritas y procesos de notificación.");

            migrationBuilder.CreateIndex(
                name: "IX_norma_suscrita_proceso_notificacion_id_norma_suscrita",
                schema: "tanatos",
                table: "norma_suscrita_proceso_notificacion",
                column: "id_norma_suscrita");

            migrationBuilder.CreateIndex(
                name: "IX_norma_suscrita_proceso_notificacion_id_proceso_automatico",
                schema: "tanatos",
                table: "norma_suscrita_proceso_notificacion",
                column: "id_proceso_automatico");

            migrationBuilder.CreateIndex(
                name: "IX_proceso_automatico_id_tipo_proceso_automatico",
                schema: "tanatos",
                table: "proceso_automatico",
                column: "id_tipo_proceso_automatico");

            migrationBuilder.CreateIndex(
                name: "IX_proceso_automatico_nombre",
                schema: "tanatos",
                table: "proceso_automatico",
                column: "nombre");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "norma_suscrita_proceso_notificacion",
                schema: "tanatos");

            migrationBuilder.DropTable(
                name: "proceso_automatico",
                schema: "tanatos");

            migrationBuilder.DropTable(
                name: "tipo_proceso_automatico",
                schema: "tanatos");

            migrationBuilder.AlterColumn<List<Dictionary<string, JsonElement>>>(
                name: "procesos_notificaciones",
                schema: "tanatos",
                table: "norma_suscrita",
                type: "jsonb",
                nullable: true,
                comment: "Procesos de notificaciones asociados a la norma suscrita.",
                oldClrType: typeof(List<ProcesoNotificacion>),
                oldType: "jsonb",
                oldComment: "Procesos de notificaciones asociados a la norma suscrita.");
        }
    }
}
