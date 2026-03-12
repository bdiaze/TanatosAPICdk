using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Migrations
{
    /// <inheritdoc />
    public partial class ColumnSuscripcionUnicaPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "suscripcion_unica",
                schema: "tanatos",
                table: "plan",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Indicador de si el plan solo permite una suscripción única por usuario.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "suscripcion_unica",
                schema: "tanatos",
                table: "plan");
        }
    }
}
