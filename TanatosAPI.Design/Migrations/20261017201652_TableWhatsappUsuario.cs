using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class TableWhatsappUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "whatsapp_usuario",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del número de Whatsapp.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sub = table.Column<string>(type: "text", nullable: false, comment: "Usuario al que pertenece el número de Whatsapp."),
                    whatsapp = table.Column<string>(type: "text", nullable: false, comment: "Número de Whatsapp."),
                    id_acceso_validacion = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del acceso generado para la validación."),
                    fecha_validacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se validó el número de Whatsapp."),
                    validado = table.Column<bool>(type: "boolean", nullable: false, comment: "Indicador de si el número de Whatsapp fue validado."),
                    hermes_id_mensaje = table.Column<string>(type: "text", nullable: true, comment: "ID del mensaje en Hermes asociado a la notificación para validación."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó el número de Whatsapp."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó el número de Whatsapp."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del número de Whatsapp.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_whatsapp_usuario", x => x.id);
                    table.ForeignKey(
                        name: "FK_whatsapp_usuario_acceso_id_acceso_validacion",
                        column: x => x.id_acceso_validacion,
                        principalSchema: "tanatos",
                        principalTable: "acceso",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene números de Whatsapp asociados a un usuario.");

            migrationBuilder.CreateIndex(
                name: "IX_whatsapp_usuario_id_acceso_validacion",
                schema: "tanatos",
                table: "whatsapp_usuario",
                column: "id_acceso_validacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "whatsapp_usuario",
                schema: "tanatos");
        }
    }
}
