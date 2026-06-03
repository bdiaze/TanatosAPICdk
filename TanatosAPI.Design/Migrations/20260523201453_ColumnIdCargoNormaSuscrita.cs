using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class ColumnIdCargoNormaSuscrita : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "id_cargo",
                schema: "tanatos",
                table: "norma_suscrita",
                type: "bigint",
                nullable: true,
                comment: "Identificador del cargo responsable de la norma.");

            migrationBuilder.CreateIndex(
                name: "IX_norma_suscrita_id_cargo",
                schema: "tanatos",
                table: "norma_suscrita",
                column: "id_cargo");

            migrationBuilder.AddForeignKey(
                name: "FK_norma_suscrita_cargo_id_cargo",
                schema: "tanatos",
                table: "norma_suscrita",
                column: "id_cargo",
                principalSchema: "tanatos",
                principalTable: "cargo",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_norma_suscrita_cargo_id_cargo",
                schema: "tanatos",
                table: "norma_suscrita");

            migrationBuilder.DropIndex(
                name: "IX_norma_suscrita_id_cargo",
                schema: "tanatos",
                table: "norma_suscrita");

            migrationBuilder.DropColumn(
                name: "id_cargo",
                schema: "tanatos",
                table: "norma_suscrita");
        }
    }
}
