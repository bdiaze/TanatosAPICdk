using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Migrations
{
    /// <inheritdoc />
    public partial class CorreccionVigencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "vigente",
                schema: "tanatos",
                table: "destinatario_notificacion",
                newName: "vigencia");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "vigencia",
                schema: "tanatos",
                table: "destinatario_notificacion",
                newName: "vigente");
        }
    }
}
