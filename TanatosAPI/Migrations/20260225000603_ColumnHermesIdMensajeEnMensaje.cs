using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Migrations
{
    /// <inheritdoc />
    public partial class ColumnHermesIdMensajeEnMensaje : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "hermes_id_mensaje",
                schema: "tanatos",
                table: "mensaje",
                type: "text",
                nullable: true,
                comment: "ID del mensaje en Hermes.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "hermes_id_mensaje",
                schema: "tanatos",
                table: "mensaje");
        }
    }
}
