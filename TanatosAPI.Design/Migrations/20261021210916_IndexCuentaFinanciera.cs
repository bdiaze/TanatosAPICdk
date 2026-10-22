using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class IndexCuentaFinanciera : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_cuenta_financiera_sub_id_negocio",
                schema: "tanatos",
                table: "cuenta_financiera",
                columns: new[] { "sub", "id_negocio" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_cuenta_financiera_sub_id_negocio",
                schema: "tanatos",
                table: "cuenta_financiera");
        }
    }
}
