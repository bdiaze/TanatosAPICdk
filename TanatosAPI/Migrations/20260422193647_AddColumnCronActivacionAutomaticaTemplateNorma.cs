using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnCronActivacionAutomaticaTemplateNorma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cron_activacion_automatica",
                schema: "tanatos",
                table: "template_norma",
                type: "text",
                nullable: true,
                comment: "Cron que define el próximo vencimiento de la obligación al momento de la inscripción.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cron_activacion_automatica",
                schema: "tanatos",
                table: "template_norma");
        }
    }
}
