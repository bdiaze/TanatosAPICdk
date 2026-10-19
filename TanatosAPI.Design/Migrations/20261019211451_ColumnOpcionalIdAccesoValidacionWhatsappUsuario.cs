using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class ColumnOpcionalIdAccesoValidacionWhatsappUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "id_acceso_validacion",
                schema: "tanatos",
                table: "whatsapp_usuario",
                type: "bigint",
                nullable: true,
                comment: "Identificador del acceso generado para la validación.",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "Identificador del acceso generado para la validación.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "id_acceso_validacion",
                schema: "tanatos",
                table: "whatsapp_usuario",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "Identificador del acceso generado para la validación.",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true,
                oldComment: "Identificador del acceso generado para la validación.");
        }
    }
}
