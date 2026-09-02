using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class RequiredUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "user_name",
                schema: "tanatos",
                table: "usuario",
                type: "text",
                nullable: false,
                defaultValue: "",
                comment: "User name del usuario.",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true,
                oldComment: "User name del usuario.");

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_creacion",
                schema: "tanatos",
                table: "usuario",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                comment: "Fecha de creación del usuario.",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true,
                oldComment: "Fecha de creación del usuario.");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "user_name",
                schema: "tanatos",
                table: "usuario",
                type: "text",
                nullable: true,
                comment: "User name del usuario.",
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "User name del usuario.");

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_creacion",
                schema: "tanatos",
                table: "usuario",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Fecha de creación del usuario.",
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldComment: "Fecha de creación del usuario.");
        }
    }
}
