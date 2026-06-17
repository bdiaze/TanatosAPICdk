using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class DiasActivacionAutomaticaTemplateNorma : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "dias_activacion_automatica",
                schema: "tanatos",
                table: "template_norma",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "dias_activacion_automatica",
                schema: "tanatos",
                table: "template_norma");
        }
    }
}
