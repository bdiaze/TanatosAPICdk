using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class ColumnWhatsappUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "whatsapp",
                schema: "tanatos",
                table: "usuario",
                type: "text",
                nullable: true,
                comment: "Número de WhatsApp del usuario.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "whatsapp",
                schema: "tanatos",
                table: "usuario");
        }
    }
}
