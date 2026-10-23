using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class AlterIndexMovimientoFinanciero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_movimiento_financiero_id_cuenta_financiera_fecha_movimiento",
                schema: "tanatos",
                table: "movimiento_financiero");

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_financiero_id_cuenta_financiera_fecha_movimiento~",
                schema: "tanatos",
                table: "movimiento_financiero",
                columns: new[] { "id_cuenta_financiera", "fecha_movimiento", "id" });

            migrationBuilder.CreateIndex(
                name: "IX_categoria_financiera_sub_id_negocio",
                schema: "tanatos",
                table: "categoria_financiera",
                columns: new[] { "sub", "id_negocio" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_movimiento_financiero_id_cuenta_financiera_fecha_movimiento~",
                schema: "tanatos",
                table: "movimiento_financiero");

            migrationBuilder.DropIndex(
                name: "IX_categoria_financiera_sub_id_negocio",
                schema: "tanatos",
                table: "categoria_financiera");

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_financiero_id_cuenta_financiera_fecha_movimiento",
                schema: "tanatos",
                table: "movimiento_financiero",
                columns: new[] { "id_cuenta_financiera", "fecha_movimiento" });
        }
    }
}
