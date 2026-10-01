using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class TableAccesoPermisoRecurso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "acceso",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del acceso.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    hash_codigo = table.Column<string>(type: "text", nullable: false, comment: "Hash SHA256 del código de acceso."),
                    fecha_expiracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha de expiración del acceso."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó el acceso."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó el acceso."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del acceso.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acceso", x => x.id);
                },
                comment: "Tabla que contiene los accesos delegados a recursos de usuarios.");

            migrationBuilder.CreateTable(
                name: "recurso",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del recurso.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipo = table.Column<string>(type: "text", nullable: false, comment: "Tipo de recurso. Por ejemplo, norma suscrita, historial_norma_suscrita, documento_adjunto."),
                    id_interno = table.Column<string>(type: "text", nullable: false, comment: "Identificador interno del recurso, por lo general es una referencia al ID de la tabla asociada al tipo de recurso."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó el recurso."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó el recurso."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del recurso.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recurso", x => x.id);
                },
                comment: "Tabla que contiene los recursos de usuarios a los que se le ha delegado acceso.");

            migrationBuilder.CreateTable(
                name: "permiso",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del permiso.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_acceso = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del acceso asociado al permiso."),
                    id_recurso = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del recurso asociado al permiso."),
                    accion = table.Column<string>(type: "text", nullable: false, comment: "Acción permitida por el acceso sobre el recurso. Por ejemplo, leer, completar, descargar, etc."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó el permiso."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó el permiso."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del permiso.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permiso", x => x.id);
                    table.ForeignKey(
                        name: "FK_permiso_acceso_id_acceso",
                        column: x => x.id_acceso,
                        principalSchema: "tanatos",
                        principalTable: "acceso",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_permiso_recurso_id_recurso",
                        column: x => x.id_recurso,
                        principalSchema: "tanatos",
                        principalTable: "recurso",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene los permisos que tiene un acceso sobre determinado recurso.");

            migrationBuilder.CreateIndex(
                name: "IX_acceso_hash_codigo",
                schema: "tanatos",
                table: "acceso",
                column: "hash_codigo");

            migrationBuilder.CreateIndex(
                name: "IX_permiso_id_acceso",
                schema: "tanatos",
                table: "permiso",
                column: "id_acceso");

            migrationBuilder.CreateIndex(
                name: "IX_permiso_id_recurso",
                schema: "tanatos",
                table: "permiso",
                column: "id_recurso");

            migrationBuilder.CreateIndex(
                name: "IX_recurso_tipo_id_interno",
                schema: "tanatos",
                table: "recurso",
                columns: new[] { "tipo", "id_interno" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "permiso",
                schema: "tanatos");

            migrationBuilder.DropTable(
                name: "acceso",
                schema: "tanatos");

            migrationBuilder.DropTable(
                name: "recurso",
                schema: "tanatos");
        }
    }
}
