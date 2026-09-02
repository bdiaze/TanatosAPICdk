using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using TanatosAPI.Entities.Models;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class DropColumnProcesosNotificacionNormaSuscrita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "procesos_notificaciones",
                schema: "tanatos",
                table: "norma_suscrita");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<ProcesoNotificacion>>(
                name: "procesos_notificaciones",
                schema: "tanatos",
                table: "norma_suscrita",
                type: "jsonb",
                nullable: false,
                comment: "Procesos de notificaciones asociados a la norma suscrita.");
        }
    }
}
