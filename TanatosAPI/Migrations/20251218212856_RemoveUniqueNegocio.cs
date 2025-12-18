using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUniqueNegocio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_negocio_sub_nombre",
                schema: "tanatos",
                table: "negocio");

            migrationBuilder.CreateIndex(
                name: "IX_negocio_sub_nombre",
                schema: "tanatos",
                table: "negocio",
                columns: new[] { "sub", "nombre" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_negocio_sub_nombre",
                schema: "tanatos",
                table: "negocio");

            migrationBuilder.CreateIndex(
                name: "IX_negocio_sub_nombre",
                schema: "tanatos",
                table: "negocio",
                columns: new[] { "sub", "nombre" },
                unique: true);
        }
    }
}
