using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnRequierePlanEmpresaTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "requiere_plan_empresa",
                schema: "tanatos",
                table: "template",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Indicador de si el template requiere de que el usuario tenga plan Empresa.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "requiere_plan_empresa",
                schema: "tanatos",
                table: "template");
        }
    }
}
