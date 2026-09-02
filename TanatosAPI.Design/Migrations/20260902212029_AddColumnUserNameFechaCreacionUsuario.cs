using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class AddColumnUserNameFechaCreacionUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "fecha_creacion",
                schema: "tanatos",
                table: "usuario",
                type: "timestamp with time zone",
                nullable: true,
                comment: "Fecha de creación del usuario.");

            migrationBuilder.AddColumn<string>(
                name: "user_name",
                schema: "tanatos",
                table: "usuario",
                type: "text",
                nullable: true,
                comment: "User name del usuario.");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_user_name",
                schema: "tanatos",
                table: "usuario",
                column: "user_name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_usuario_user_name",
                schema: "tanatos",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "fecha_creacion",
                schema: "tanatos",
                table: "usuario");

            migrationBuilder.DropColumn(
                name: "user_name",
                schema: "tanatos",
                table: "usuario");
        }
    }
}
