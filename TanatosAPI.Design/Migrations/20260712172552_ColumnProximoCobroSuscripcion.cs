using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class ColumnProximoCobroSuscripcion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_proximo_cobro",
                schema: "tanatos",
                table: "suscripcion",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Fecha del próximo cobro de la suscripción.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "fecha_proximo_cobro",
                schema: "tanatos",
                table: "suscripcion");
        }
    }
}
