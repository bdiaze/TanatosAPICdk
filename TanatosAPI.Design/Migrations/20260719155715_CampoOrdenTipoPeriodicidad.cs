using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class CampoOrdenTipoPeriodicidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "orden",
                schema: "tanatos",
                table: "tipo_periodicidad",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Orden visual de la periodicidad.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "orden",
                schema: "tanatos",
                table: "tipo_periodicidad");
        }
    }
}
