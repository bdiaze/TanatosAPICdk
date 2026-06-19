using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class TablePreguntaFrecuente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pregunta_frecuente",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador de la pregunta frecuente.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    pregunta = table.Column<string>(type: "text", nullable: false, comment: "Título de la pregunta frecuente."),
                    respuesta = table.Column<string>(type: "text", nullable: false, comment: "Respuesta a la pregunta frecuente."),
                    habilitado = table.Column<bool>(type: "boolean", nullable: false, comment: "Indicador de si la pregunta frecuente está habilitada."),
                    orden = table.Column<int>(type: "integer", nullable: false, comment: "Orden en que se presenta la pregunta frecuente."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó la pregunta frecuente."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó la pregunta frecuente."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia de la pregunta frecuente.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pregunta_frecuente", x => x.id);
                },
                comment: "Tabla que contiene las preguntas frecuentes con sus respectivas respuestas.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pregunta_frecuente",
                schema: "tanatos");
        }
    }
}
