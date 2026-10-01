using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class UniqueIndexAcceso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_acceso_hash_codigo",
                schema: "tanatos",
                table: "acceso");

            migrationBuilder.CreateIndex(
                name: "IX_acceso_hash_codigo",
                schema: "tanatos",
                table: "acceso",
                column: "hash_codigo",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_acceso_hash_codigo",
                schema: "tanatos",
                table: "acceso");

            migrationBuilder.CreateIndex(
                name: "IX_acceso_hash_codigo",
                schema: "tanatos",
                table: "acceso",
                column: "hash_codigo");
        }
    }
}
