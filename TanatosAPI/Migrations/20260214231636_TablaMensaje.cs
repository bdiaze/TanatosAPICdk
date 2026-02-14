using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TanatosAPI.Migrations
{
    /// <inheritdoc />
    public partial class TablaMensaje : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "mensaje",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador de la notificación asociada a una norma suscrita.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sub = table.Column<string>(type: "text", nullable: true, comment: "Usuario que ingresó el mensaje."),
                    nombre = table.Column<string>(type: "text", nullable: false, comment: "Nombre del usuario que ingresó el mensaje."),
                    correo = table.Column<string>(type: "text", nullable: false, comment: "Correo electrónico del usuario que ingresó el mensaje."),
                    contenido = table.Column<string>(type: "text", nullable: false, comment: "Contenido del mensaje."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó el mensaje.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mensaje", x => x.id);
                },
                comment: "Tabla que contiene los mensajes ingresados por formulario de contacto.");

            migrationBuilder.CreateIndex(
                name: "IX_mensaje_correo",
                schema: "tanatos",
                table: "mensaje",
                column: "correo");

            migrationBuilder.CreateIndex(
                name: "IX_mensaje_fecha_creacion",
                schema: "tanatos",
                table: "mensaje",
                column: "fecha_creacion");

            migrationBuilder.CreateIndex(
                name: "IX_mensaje_sub",
                schema: "tanatos",
                table: "mensaje",
                column: "sub");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "mensaje",
                schema: "tanatos");
        }
    }
}
