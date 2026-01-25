using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Migrations
{
    /// <inheritdoc />
    public partial class DeltasPeriodicidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "delta_annos",
                schema: "tanatos",
                table: "tipo_periodicidad",
                type: "integer",
                nullable: true,
                comment: "Delta en años de la periodicidad.");

            migrationBuilder.AddColumn<int>(
                name: "delta_dias",
                schema: "tanatos",
                table: "tipo_periodicidad",
                type: "integer",
                nullable: true,
                comment: "Delta en días de la periodicidad.");

            migrationBuilder.AddColumn<int>(
                name: "delta_meses",
                schema: "tanatos",
                table: "tipo_periodicidad",
                type: "integer",
                nullable: true,
                comment: "Delta en meses de la periodicidad.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "delta_annos",
                schema: "tanatos",
                table: "tipo_periodicidad");

            migrationBuilder.DropColumn(
                name: "delta_dias",
                schema: "tanatos",
                table: "tipo_periodicidad");

            migrationBuilder.DropColumn(
                name: "delta_meses",
                schema: "tanatos",
                table: "tipo_periodicidad");
        }
    }
}
