using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class TableEmpleado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "empleado",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del empleado.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sub = table.Column<string>(type: "text", nullable: false, comment: "Usuario al que pertenece el empleado."),
                    id_negocio = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del negocio del usuario."),
                    nombre = table.Column<string>(type: "text", nullable: false, comment: "Nombre del empleado."),
                    id_cargo = table.Column<long>(type: "bigint", nullable: true, comment: "Identificador del cargo del empleado."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó el empleado."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó el empleado."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del empleado.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empleado", x => x.id);
                    table.ForeignKey(
                        name: "FK_empleado_cargo_id_cargo",
                        column: x => x.id_cargo,
                        principalSchema: "tanatos",
                        principalTable: "cargo",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_empleado_negocio_id_negocio",
                        column: x => x.id_negocio,
                        principalSchema: "tanatos",
                        principalTable: "negocio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene los empleados asociados a un negocio.");

            migrationBuilder.CreateIndex(
                name: "IX_empleado_id_cargo",
                schema: "tanatos",
                table: "empleado",
                column: "id_cargo");

            migrationBuilder.CreateIndex(
                name: "IX_empleado_id_negocio",
                schema: "tanatos",
                table: "empleado",
                column: "id_negocio");

            migrationBuilder.CreateIndex(
                name: "IX_empleado_sub_id_negocio",
                schema: "tanatos",
                table: "empleado",
                columns: new[] { "sub", "id_negocio" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "empleado",
                schema: "tanatos");
        }
    }
}
