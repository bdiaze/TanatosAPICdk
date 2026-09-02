using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class DropColumnFechaEliminacionVigenciaTipoProcesoAutomatico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fecha_eliminacion",
                schema: "tanatos",
                table: "tipo_proceso_automatico");

            migrationBuilder.DropColumn(
                name: "vigencia",
                schema: "tanatos",
                table: "tipo_proceso_automatico");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_eliminacion",
                schema: "tanatos",
                table: "tipo_proceso_automatico",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Fecha en que se eliminó el tipo de proceso automático.");

            migrationBuilder.AddColumn<bool>(
                name: "vigencia",
                schema: "tanatos",
                table: "tipo_proceso_automatico",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                comment: "Vigencia del tipo de proceso automático.");
        }
    }
}
