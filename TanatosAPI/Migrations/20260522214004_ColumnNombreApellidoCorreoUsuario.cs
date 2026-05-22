using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Migrations
{
    /// <inheritdoc />
    public partial class ColumnNombreApellidoCorreoUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "apellido",
                schema: "tanatos",
                table: "usuario",
                type: "text",
                nullable: true,
                comment: "Apellido del usuario.");

            migrationBuilder.AddColumn<string>(
                name: "correo_electronico",
                schema: "tanatos",
                table: "usuario",
                type: "text",
                nullable: true,
                comment: "Correo electrónico del usuario.");

            migrationBuilder.AddColumn<string>(
                name: "nombre",
                schema: "tanatos",
                table: "usuario",
                type: "text",
                nullable: true,
                comment: "Nombre del usuario.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "apellido",
                schema: "tanatos",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "correo_electronico",
                schema: "tanatos",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "nombre",
                schema: "tanatos",
                table: "usuario");
        }
    }
}
