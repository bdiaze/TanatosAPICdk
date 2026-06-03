using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class TableCargo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cargo",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del cargo.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sub = table.Column<string>(type: "text", nullable: false, comment: "Usuario al que pertenece el cargo."),
                    id_negocio = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del negocio del usuario."),
                    nombre = table.Column<string>(type: "text", nullable: false, comment: "Nombre del cargo."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó el cargo."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó el cargo."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del cargo.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cargo", x => x.id);
                    table.ForeignKey(
                        name: "FK_cargo_negocio_id_negocio",
                        column: x => x.id_negocio,
                        principalSchema: "tanatos",
                        principalTable: "negocio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene los cargos asociados a un negocio.");

            migrationBuilder.CreateIndex(
                name: "IX_cargo_id_negocio",
                schema: "tanatos",
                table: "cargo",
                column: "id_negocio");

            migrationBuilder.CreateIndex(
                name: "IX_cargo_sub_id_negocio",
                schema: "tanatos",
                table: "cargo",
                columns: new[] { "sub", "id_negocio" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cargo",
                schema: "tanatos");
        }
    }
}
