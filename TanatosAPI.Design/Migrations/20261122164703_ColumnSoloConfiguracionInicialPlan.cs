using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class ColumnSoloConfiguracionInicialPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "solo_configuracion_inicial",
                schema: "tanatos",
                table: "plan",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "solo_configuracion_inicial",
                schema: "tanatos",
                table: "plan");
        }
    }
}
