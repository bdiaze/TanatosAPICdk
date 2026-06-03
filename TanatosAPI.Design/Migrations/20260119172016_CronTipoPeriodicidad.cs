using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class CronTipoPeriodicidad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cron",
                schema: "tanatos",
                table: "tipo_periodicidad",
                type: "text",
                nullable: true,
                comment: "Cron del tipo de periodicidad.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "cron",
                schema: "tanatos",
                table: "tipo_periodicidad");
        }
    }
}
