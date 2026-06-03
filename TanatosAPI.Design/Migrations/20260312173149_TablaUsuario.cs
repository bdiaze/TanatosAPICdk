using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class TablaUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "usuario",
                schema: "tanatos",
                columns: table => new
                {
                    sub = table.Column<string>(type: "text", nullable: false, comment: "Identificador del usuario."),
                    flow_customer_id = table.Column<string>(type: "text", nullable: true, comment: "ID del cliente en Flow.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.sub);
                },
                comment: "Tabla que contiene la información del usuario.");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_flow_customer_id",
                schema: "tanatos",
                table: "usuario",
                column: "flow_customer_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "usuario",
                schema: "tanatos");
        }
    }
}
