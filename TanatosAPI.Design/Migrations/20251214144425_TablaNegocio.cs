using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class TablaNegocio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "negocio",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del negocio.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sub = table.Column<string>(type: "text", nullable: false, comment: "Usuario al que pertenece el negocio."),
                    nombre = table.Column<string>(type: "text", nullable: false, comment: "Nombre del negocio."),
                    direccion = table.Column<string>(type: "text", nullable: true, comment: "Dirección del negocio."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó el negocio."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó el negocio."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del negocio.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_negocio", x => x.id);
                },
                comment: "Tabla que contiene los negocios de un usuario.");

            migrationBuilder.CreateIndex(
                name: "IX_negocio_sub",
                schema: "tanatos",
                table: "negocio",
                column: "sub");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "negocio",
                schema: "tanatos");
        }
    }
}
