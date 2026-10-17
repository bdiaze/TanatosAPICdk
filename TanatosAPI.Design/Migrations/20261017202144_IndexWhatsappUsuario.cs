using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class IndexWhatsappUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_whatsapp_usuario_sub",
                schema: "tanatos",
                table: "whatsapp_usuario",
                column: "sub");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_whatsapp_usuario_sub",
                schema: "tanatos",
                table: "whatsapp_usuario");
        }
    }
}
