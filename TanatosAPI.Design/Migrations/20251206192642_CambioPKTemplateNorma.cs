using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class CambioPKTemplateNorma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_template_norma_fiscalizador_template_norma_id_template_norma",
                schema: "tanatos",
                table: "template_norma_fiscalizador");

            migrationBuilder.DropForeignKey(
                name: "FK_template_norma_notificacion_template_norma_id_template_norma",
                schema: "tanatos",
                table: "template_norma_notificacion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_template_norma_notificacion",
                schema: "tanatos",
                table: "template_norma_notificacion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_template_norma_fiscalizador",
                schema: "tanatos",
                table: "template_norma_fiscalizador");

            migrationBuilder.DropPrimaryKey(
                name: "PK_template_norma",
                schema: "tanatos",
                table: "template_norma");

            migrationBuilder.DropIndex(
                name: "IX_template_norma_id_template",
                schema: "tanatos",
                table: "template_norma");

            migrationBuilder.DropColumn(
                name: "id_template_norma",
                schema: "tanatos",
                table: "template_norma_notificacion");

            migrationBuilder.DropColumn(
                name: "id_template_norma",
                schema: "tanatos",
                table: "template_norma_fiscalizador");

            migrationBuilder.RenameColumn(
                name: "id",
                schema: "tanatos",
                table: "template_norma",
                newName: "id_norma");

            migrationBuilder.AddColumn<long>(
                name: "id_template",
                schema: "tanatos",
                table: "template_norma_notificacion",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "Identificador del template al que pertenece la norma.");

            migrationBuilder.AddColumn<long>(
                name: "id_norma",
                schema: "tanatos",
                table: "template_norma_notificacion",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "Identificador de la norma asociada al template.");

            migrationBuilder.AddColumn<long>(
                name: "id_template",
                schema: "tanatos",
                table: "template_norma_fiscalizador",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "Identificador del template al que pertenece la norma.");

            migrationBuilder.AddColumn<long>(
                name: "id_norma",
                schema: "tanatos",
                table: "template_norma_fiscalizador",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "Identificador de la norma asociada al template.");

            migrationBuilder.AddPrimaryKey(
                name: "PK_template_norma_notificacion",
                schema: "tanatos",
                table: "template_norma_notificacion",
                columns: new[] { "id_template", "id_norma", "id_tipo_unidad_tiempo_antelacion", "cant_antelacion" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_template_norma_fiscalizador",
                schema: "tanatos",
                table: "template_norma_fiscalizador",
                columns: new[] { "id_template", "id_norma", "id_tipo_fiscalizador" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_template_norma",
                schema: "tanatos",
                table: "template_norma",
                columns: new[] { "id_template", "id_norma" });

            migrationBuilder.AddForeignKey(
                name: "FK_template_norma_fiscalizador_template_norma_id_template_id_n~",
                schema: "tanatos",
                table: "template_norma_fiscalizador",
                columns: new[] { "id_template", "id_norma" },
                principalSchema: "tanatos",
                principalTable: "template_norma",
                principalColumns: new[] { "id_template", "id_norma" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_template_norma_notificacion_template_norma_id_template_id_n~",
                schema: "tanatos",
                table: "template_norma_notificacion",
                columns: new[] { "id_template", "id_norma" },
                principalSchema: "tanatos",
                principalTable: "template_norma",
                principalColumns: new[] { "id_template", "id_norma" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_template_norma_fiscalizador_template_norma_id_template_id_n~",
                schema: "tanatos",
                table: "template_norma_fiscalizador");

            migrationBuilder.DropForeignKey(
                name: "FK_template_norma_notificacion_template_norma_id_template_id_n~",
                schema: "tanatos",
                table: "template_norma_notificacion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_template_norma_notificacion",
                schema: "tanatos",
                table: "template_norma_notificacion");

            migrationBuilder.DropPrimaryKey(
                name: "PK_template_norma_fiscalizador",
                schema: "tanatos",
                table: "template_norma_fiscalizador");

            migrationBuilder.DropPrimaryKey(
                name: "PK_template_norma",
                schema: "tanatos",
                table: "template_norma");

            migrationBuilder.DropColumn(
                name: "id_template",
                schema: "tanatos",
                table: "template_norma_notificacion");

            migrationBuilder.DropColumn(
                name: "id_norma",
                schema: "tanatos",
                table: "template_norma_notificacion");

            migrationBuilder.DropColumn(
                name: "id_template",
                schema: "tanatos",
                table: "template_norma_fiscalizador");

            migrationBuilder.DropColumn(
                name: "id_norma",
                schema: "tanatos",
                table: "template_norma_fiscalizador");

            migrationBuilder.RenameColumn(
                name: "id_norma",
                schema: "tanatos",
                table: "template_norma",
                newName: "id");

            migrationBuilder.AddColumn<long>(
                name: "id_template_norma",
                schema: "tanatos",
                table: "template_norma_notificacion",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "Identificador de la template norma.");

            migrationBuilder.AddColumn<long>(
                name: "id_template_norma",
                schema: "tanatos",
                table: "template_norma_fiscalizador",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "Identificador de la norma perteneciente a un template.");

            migrationBuilder.AddPrimaryKey(
                name: "PK_template_norma_notificacion",
                schema: "tanatos",
                table: "template_norma_notificacion",
                columns: new[] { "id_template_norma", "id_tipo_unidad_tiempo_antelacion" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_template_norma_fiscalizador",
                schema: "tanatos",
                table: "template_norma_fiscalizador",
                columns: new[] { "id_template_norma", "id_tipo_fiscalizador" });

            migrationBuilder.AddPrimaryKey(
                name: "PK_template_norma",
                schema: "tanatos",
                table: "template_norma",
                column: "id");

            migrationBuilder.CreateIndex(
                name: "IX_template_norma_id_template",
                schema: "tanatos",
                table: "template_norma",
                column: "id_template");

            migrationBuilder.AddForeignKey(
                name: "FK_template_norma_fiscalizador_template_norma_id_template_norma",
                schema: "tanatos",
                table: "template_norma_fiscalizador",
                column: "id_template_norma",
                principalSchema: "tanatos",
                principalTable: "template_norma",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_template_norma_notificacion_template_norma_id_template_norma",
                schema: "tanatos",
                table: "template_norma_notificacion",
                column: "id_template_norma",
                principalSchema: "tanatos",
                principalTable: "template_norma",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
