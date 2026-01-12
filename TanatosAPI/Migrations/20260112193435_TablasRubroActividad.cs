using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Migrations
{
    /// <inheritdoc />
    public partial class TablasRubroActividad : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "id_tipo_actividad",
                schema: "tanatos",
                table: "negocio",
                type: "bigint",
                nullable: true,
                comment: "Identificador de la actividad que efectúa el negocio.");

            migrationBuilder.CreateTable(
                name: "tipo_rubro",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del rubro."),
                    nombre = table.Column<string>(type: "text", nullable: false, comment: "Nombre del rubro."),
                    descripcion = table.Column<string>(type: "text", nullable: true, comment: "Descripción del rubro."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del rubro.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipo_rubro", x => x.id);
                },
                comment: "Tabla que contiene los rubros a los que puede pertenecer un negocio.");

            migrationBuilder.CreateTable(
                name: "tipo_actividad",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador de la actividad."),
                    id_tipo_rubro = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del rubro al que pertenece la actividad."),
                    nombre = table.Column<string>(type: "text", nullable: false, comment: "Nombre de la actividad."),
                    descripcion = table.Column<string>(type: "text", nullable: true, comment: "Descripción de la actividad."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia de la actividad.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipo_actividad", x => x.id);
                    table.ForeignKey(
                        name: "FK_tipo_actividad_tipo_rubro_id_tipo_rubro",
                        column: x => x.id_tipo_rubro,
                        principalSchema: "tanatos",
                        principalTable: "tipo_rubro",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene las actividades que puede hacer un negocio.");

            migrationBuilder.CreateIndex(
                name: "IX_negocio_id_tipo_actividad",
                schema: "tanatos",
                table: "negocio",
                column: "id_tipo_actividad");

            migrationBuilder.CreateIndex(
                name: "IX_tipo_actividad_id_tipo_rubro",
                schema: "tanatos",
                table: "tipo_actividad",
                column: "id_tipo_rubro");

            migrationBuilder.AddForeignKey(
                name: "FK_negocio_tipo_actividad_id_tipo_actividad",
                schema: "tanatos",
                table: "negocio",
                column: "id_tipo_actividad",
                principalSchema: "tanatos",
                principalTable: "tipo_actividad",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_negocio_tipo_actividad_id_tipo_actividad",
                schema: "tanatos",
                table: "negocio");

            migrationBuilder.DropTable(
                name: "tipo_actividad",
                schema: "tanatos");

            migrationBuilder.DropTable(
                name: "tipo_rubro",
                schema: "tanatos");

            migrationBuilder.DropIndex(
                name: "IX_negocio_id_tipo_actividad",
                schema: "tanatos",
                table: "negocio");

            migrationBuilder.DropColumn(
                name: "id_tipo_actividad",
                schema: "tanatos",
                table: "negocio");
        }
    }
}
