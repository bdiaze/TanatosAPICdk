using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "tanatos");

            migrationBuilder.CreateTable(
                name: "tipo_receptor_notificacion",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    vigente = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipo_receptor_notificacion", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tipo_receptor_notificacion",
                schema: "tanatos");
        }
    }
}
