using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class RegexValidacionTipoReceptor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "regex_validacion",
                schema: "tanatos",
                table: "tipo_receptor_notificacion",
                type: "text",
                nullable: true,
                comment: "Regex para validar el tipo de receptor.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "regex_validacion",
                schema: "tanatos",
                table: "tipo_receptor_notificacion");
        }
    }
}
