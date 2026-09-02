using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class ColumnMisionVisionValoresNegocio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "mision",
                schema: "tanatos",
                table: "negocio",
                type: "text",
                nullable: true,
                comment: "Misión o propósito central del negocio.");

            migrationBuilder.AddColumn<string>(
                name: "valores",
                schema: "tanatos",
                table: "negocio",
                type: "text",
                nullable: true,
                comment: "Valores o ideales con los que se identifica el negocio.");

            migrationBuilder.AddColumn<string>(
                name: "vision",
                schema: "tanatos",
                table: "negocio",
                type: "text",
                nullable: true,
                comment: "Visión o aspiraciones del negocio.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "mision",
                schema: "tanatos",
                table: "negocio");

            migrationBuilder.DropColumn(
                name: "valores",
                schema: "tanatos",
                table: "negocio");

            migrationBuilder.DropColumn(
                name: "vision",
                schema: "tanatos",
                table: "negocio");
        }
    }
}
