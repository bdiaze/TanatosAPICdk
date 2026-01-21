using System.Collections.Generic;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Migrations
{
    /// <inheritdoc />
    public partial class ColumnProcesoNotificacionNormaSuscrita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<Dictionary<string, JsonElement>>>(
                name: "procesos_notificaciones",
                schema: "tanatos",
                table: "norma_suscrita",
                type: "jsonb",
                nullable: true,
                comment: "Procesos de notificaciones asociados a la norma suscrita.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "procesos_notificaciones",
                schema: "tanatos",
                table: "norma_suscrita");
        }
    }
}
