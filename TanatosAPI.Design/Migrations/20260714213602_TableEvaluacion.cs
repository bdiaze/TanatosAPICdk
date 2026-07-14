using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class TableEvaluacion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "evaluacion",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador de la evaluación.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sub = table.Column<string>(type: "text", nullable: false, comment: "Identificador del usuario quien emitió la evaluación."),
                    puntaje = table.Column<short>(type: "smallint", nullable: false, comment: "Puntaje que dejó el usuario."),
                    comentario = table.Column<string>(type: "text", nullable: true, comment: "Comentario que dejó el usuario."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se emitió la evaluación.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evaluacion", x => x.id);
                },
                comment: "Tabla que contiene las evaluaciones emitidas por los usuarios.");

            migrationBuilder.CreateIndex(
                name: "IX_evaluacion_fecha_creacion",
                schema: "tanatos",
                table: "evaluacion",
                column: "fecha_creacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "evaluacion",
                schema: "tanatos");
        }
    }
}
