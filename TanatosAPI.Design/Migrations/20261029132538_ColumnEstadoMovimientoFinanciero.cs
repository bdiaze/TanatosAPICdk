using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class ColumnEstadoMovimientoFinanciero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "estado",
                schema: "tanatos",
                table: "movimiento_financiero",
                type: "smallint",
                nullable: false,
                defaultValue: (short)1,
                comment: "Estado del movimiento - 0: Preliminar - 1: Confirmado.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "estado",
                schema: "tanatos",
                table: "movimiento_financiero");
        }
    }
}
