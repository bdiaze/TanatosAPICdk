using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class CommentDiasActivacionAutomaticaTemplateNorma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "dias_activacion_automatica",
                schema: "tanatos",
                table: "template_norma",
                type: "integer",
                nullable: true,
                comment: "Días que define el próximo vencimiento de la obligación al momento de la inscripción.",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "dias_activacion_automatica",
                schema: "tanatos",
                table: "template_norma",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true,
                oldComment: "Días que define el próximo vencimiento de la obligación al momento de la inscripción.");
        }
    }
}
