using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class ColumnPrecioPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "precio_anual",
                schema: "tanatos",
                table: "plan");

            migrationBuilder.DropColumn(
                name: "precio_mensual",
                schema: "tanatos",
                table: "plan");

            migrationBuilder.AddColumn<decimal>(
                name: "precio",
                schema: "tanatos",
                table: "plan",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                comment: "Precio del plan.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "precio",
                schema: "tanatos",
                table: "plan");

            migrationBuilder.AddColumn<decimal>(
                name: "precio_anual",
                schema: "tanatos",
                table: "plan",
                type: "numeric",
                nullable: true,
                comment: "Precio anual del plan.");

            migrationBuilder.AddColumn<decimal>(
                name: "precio_mensual",
                schema: "tanatos",
                table: "plan",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                comment: "Precio mensual del plan.");
        }
    }
}
