using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class NuloDescripcionVideoTutorial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "descripcion",
                schema: "tanatos",
                table: "video_tutorial",
                type: "text",
                nullable: true,
                comment: "Descripción del video tutorial.",
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "Descripción del video tutorial.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "descripcion",
                schema: "tanatos",
                table: "video_tutorial",
                type: "text",
                nullable: false,
                defaultValue: "",
                comment: "Descripción del video tutorial.",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "Descripción del video tutorial.");
        }
    }
}
