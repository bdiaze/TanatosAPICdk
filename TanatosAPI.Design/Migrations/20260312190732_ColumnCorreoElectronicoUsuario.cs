using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class ColumnCorreoElectronicoUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "correo_electronico",
                schema: "tanatos",
                table: "usuario",
                type: "text",
                nullable: false,
                defaultValue: "",
                comment: "Correo electrónico del cliente.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "correo_electronico",
                schema: "tanatos",
                table: "usuario");
        }
    }
}
