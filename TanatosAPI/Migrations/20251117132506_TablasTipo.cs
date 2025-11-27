using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TanatosAPI.Migrations
{
    /// <inheritdoc />
    public partial class TablasTipo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "vigente",
                schema: "tanatos",
                table: "tipo_receptor_notificacion");

            migrationBuilder.AlterTable(
                name: "tipo_receptor_notificacion",
                schema: "tanatos",
                comment: "Tabla que contiene los tipos de receptores de notificación.");

            migrationBuilder.AlterColumn<string>(
                name: "nombre",
                schema: "tanatos",
                table: "tipo_receptor_notificacion",
                type: "text",
                nullable: false,
                comment: "Nombre del tipo de receptor de notificación.",
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "tanatos",
                table: "tipo_receptor_notificacion",
                type: "bigint",
                nullable: false,
                comment: "Identificador del tipo de receptor de notificación.",
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<bool>(
                name: "vigencia",
                schema: "tanatos",
                table: "tipo_receptor_notificacion",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Vigencia del tipo de receptor de notificación.");

            migrationBuilder.CreateTable(
                name: "categoria_norma",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador de la categoría."),
                    nombre = table.Column<string>(type: "text", nullable: false, comment: "Nombre de la categoría."),
                    nombre_corto = table.Column<string>(type: "text", nullable: true, comment: "Nombre corto de la categoría."),
                    descripcion = table.Column<string>(type: "text", nullable: true, comment: "Descripción de la categoría."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia de la categoría.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categoria_norma", x => x.id);
                },
                comment: "Tabla que contiene las categorías de las normas");

            migrationBuilder.CreateTable(
                name: "destinatario_notificacion",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del destinatario de notificación.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sub = table.Column<string>(type: "text", nullable: false, comment: "Usuario al que pertenece el destinatario de notificación."),
                    id_tipo_receptor = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del tipo de receptor asociado al destinatario."),
                    destino = table.Column<string>(type: "text", nullable: false, comment: "Destino de la notificación. Puede ser un correo o un número de Whatsapp."),
                    codigo_validacion = table.Column<string>(type: "text", nullable: false, comment: "Código generado para validar que el destinatario se suscribe a la notificación."),
                    intentos_validacion = table.Column<short>(type: "smallint", nullable: false, comment: "Cantidad de intentos de validar al destinatario."),
                    validado = table.Column<bool>(type: "boolean", nullable: false, comment: "Identifica si el destinatario ya fue validado."),
                    fecha_creacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó el destinatario."),
                    fecha_eliminacion = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se eliminó el destinatario."),
                    vigente = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del destinatario.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_destinatario_notificacion", x => x.id);
                    table.ForeignKey(
                        name: "FK_destinatario_notificacion_tipo_receptor_notificacion_id_tip~",
                        column: x => x.id_tipo_receptor,
                        principalSchema: "tanatos",
                        principalTable: "tipo_receptor_notificacion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene los destinatarios de las notificaciones de un usuario.");

            migrationBuilder.CreateTable(
                name: "tipo_fiscalizador",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del tipo de fiscalizador."),
                    nombre = table.Column<string>(type: "text", nullable: false, comment: "Nombre del tipo de fiscalizador."),
                    nombre_corto = table.Column<string>(type: "text", nullable: true, comment: "Nombre corto del tipo de fiscalizador."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del tipo de fiscalizador.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipo_fiscalizador", x => x.id);
                },
                comment: "Tabla que contiene los tipos de fiscalizadores de las normas.");

            migrationBuilder.CreateTable(
                name: "tipo_periodicidad",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del tipo de periodicidad."),
                    nombre = table.Column<string>(type: "text", nullable: false, comment: "Nombre del tipo de periodicidad."),
                    descripcion = table.Column<string>(type: "text", nullable: true, comment: "Descripción del tipo de periodicidad."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del tipo de periodicidad.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipo_periodicidad", x => x.id);
                },
                comment: "Tabla que contiene los tipos de periodicidad.");

            migrationBuilder.CreateTable(
                name: "tipo_unidad_tiempo",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del tipo de unidad de tiempo."),
                    nombre = table.Column<string>(type: "text", nullable: false, comment: "Nombre del tipo de unidad de tiempo."),
                    cant_segundos = table.Column<long>(type: "bigint", nullable: false, comment: "Cantidad de segundos que representan a la unidad de tiempo."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del tipo de unidad de tiempo.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipo_unidad_tiempo", x => x.id);
                },
                comment: "Tabla que contiene los tipos de unidades de tiempo.");

            migrationBuilder.CreateIndex(
                name: "IX_destinatario_notificacion_id_tipo_receptor",
                schema: "tanatos",
                table: "destinatario_notificacion",
                column: "id_tipo_receptor");

            migrationBuilder.CreateIndex(
                name: "IX_destinatario_notificacion_sub_id_tipo_receptor",
                schema: "tanatos",
                table: "destinatario_notificacion",
                columns: new[] { "sub", "id_tipo_receptor" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "categoria_norma",
                schema: "tanatos");

            migrationBuilder.DropTable(
                name: "destinatario_notificacion",
                schema: "tanatos");

            migrationBuilder.DropTable(
                name: "tipo_fiscalizador",
                schema: "tanatos");

            migrationBuilder.DropTable(
                name: "tipo_periodicidad",
                schema: "tanatos");

            migrationBuilder.DropTable(
                name: "tipo_unidad_tiempo",
                schema: "tanatos");

            migrationBuilder.DropColumn(
                name: "vigencia",
                schema: "tanatos",
                table: "tipo_receptor_notificacion");

            migrationBuilder.AlterTable(
                name: "tipo_receptor_notificacion",
                schema: "tanatos",
                oldComment: "Tabla que contiene los tipos de receptores de notificación.");

            migrationBuilder.AlterColumn<string>(
                name: "nombre",
                schema: "tanatos",
                table: "tipo_receptor_notificacion",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Nombre del tipo de receptor de notificación.");

            migrationBuilder.AlterColumn<long>(
                name: "id",
                schema: "tanatos",
                table: "tipo_receptor_notificacion",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "Identificador del tipo de receptor de notificación.");

            migrationBuilder.AddColumn<bool>(
                name: "vigente",
                schema: "tanatos",
                table: "tipo_receptor_notificacion",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
