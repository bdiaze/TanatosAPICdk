using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnPlanEmpresaTipoReceptor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "requiere_plan_empresa",
                schema: "tanatos",
                table: "tipo_receptor_notificacion",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Indicador de si el tipo de receptor requiere de que el usuario tenga plan Empresa.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "requiere_plan_empresa",
                schema: "tanatos",
                table: "tipo_receptor_notificacion");
        }
    }
}
