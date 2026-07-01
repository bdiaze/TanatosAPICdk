using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class TablaVideoTutorial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "video_tutorial",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del video tutorial.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    titulo = table.Column<string>(type: "text", nullable: false, comment: "Título del video tutorial."),
                    descripcion = table.Column<string>(type: "text", nullable: false, comment: "Descripción del video tutorial."),
                    url = table.Column<string>(type: "text", nullable: false, comment: "URL del video tutorial."),
                    habilitado = table.Column<bool>(type: "boolean", nullable: false, comment: "Indicador de si el video tutorial está habilitado."),
                    orden = table.Column<int>(type: "integer", nullable: false, comment: "Orden en que se presenta el video tutorial."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó el video tutorial."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó el video tutorial."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del video tutorial.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_video_tutorial", x => x.id);
                },
                comment: "Tabla que contiene los videos tutoriales.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "video_tutorial",
                schema: "tanatos");
        }
    }
}
