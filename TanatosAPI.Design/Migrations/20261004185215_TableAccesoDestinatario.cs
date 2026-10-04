using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class TableAccesoDestinatario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "acceso_destinatario",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador de la relación.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_acceso = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del acceso."),
                    id_destinatario_notificacion = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del destinatario de notificación."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó la relación."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó la relación."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia de la relación.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_acceso_destinatario", x => x.id);
                    table.ForeignKey(
                        name: "FK_acceso_destinatario_acceso_id_acceso",
                        column: x => x.id_acceso,
                        principalSchema: "tanatos",
                        principalTable: "acceso",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_acceso_destinatario_destinatario_notificacion_id_destinatar~",
                        column: x => x.id_destinatario_notificacion,
                        principalSchema: "tanatos",
                        principalTable: "destinatario_notificacion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene los accesos asociados a un destinatario de notificación.");

            migrationBuilder.CreateIndex(
                name: "IX_acceso_destinatario_id_acceso",
                schema: "tanatos",
                table: "acceso_destinatario",
                column: "id_acceso");

            migrationBuilder.CreateIndex(
                name: "IX_acceso_destinatario_id_destinatario_notificacion",
                schema: "tanatos",
                table: "acceso_destinatario",
                column: "id_destinatario_notificacion");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "acceso_destinatario",
                schema: "tanatos");
        }
    }
}
