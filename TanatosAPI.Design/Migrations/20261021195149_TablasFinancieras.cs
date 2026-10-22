using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class TablasFinancieras : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categoria_financiera",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador de la categoría financiera.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sub = table.Column<string>(type: "text", nullable: false, comment: "Identificador del usuario al que pertenece la categoría financiera."),
                    id_negocio = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del negocio al que pertenece la categoría financiera."),
                    nombre = table.Column<string>(type: "text", nullable: false, comment: "Nombre de la categoría financiera."),
                    tipo_movimiento = table.Column<short>(type: "smallint", nullable: false, comment: "Tipo de movimiento permitido por la categoría financiera - 1: Ingreso - 2: Gasto - 3: Ambos."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó la categoría financiera."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó la categoría financiera."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia de la categoría financiera.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categoria_financiera", x => x.id);
                    table.ForeignKey(
                        name: "FK_categoria_financiera_negocio_id_negocio",
                        column: x => x.id_negocio,
                        principalSchema: "tanatos",
                        principalTable: "negocio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene las categorías financieras de un usuario.");

            migrationBuilder.CreateTable(
                name: "tipo_cuenta_financiera",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del tipo de cuenta financiera."),
                    nombre = table.Column<string>(type: "text", nullable: false, comment: "Nombre del tipo de cuenta financiera."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del tipo de cuenta financiera.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipo_cuenta_financiera", x => x.id);
                },
                comment: "Tabla que contiene los tipos de cuentas financieras.");

            migrationBuilder.CreateTable(
                name: "cuenta_financiera",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador de la cuenta financiera.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sub = table.Column<string>(type: "text", nullable: false, comment: "Identificador del usuario al que pertenece la cuenta financiera."),
                    id_negocio = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del negocio al que pertenece la cuenta financiera."),
                    nombre = table.Column<string>(type: "text", nullable: false, comment: "Nombre de la cuenta financiera."),
                    id_tipo_cuenta_financiera = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del tipo de cuenta financiera."),
                    saldo_inicial = table.Column<decimal>(type: "numeric", nullable: false, comment: "Saldo inicial que tenía la cuenta."),
                    fecha_saldo_inicial = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se registra el saldo inicial."),
                    saldo_actual = table.Column<decimal>(type: "numeric", nullable: false, comment: "Saldo actual que tiene la cuenta."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó la cuenta financiera."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó la cuenta financiera."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia de la cuenta financiera.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cuenta_financiera", x => x.id);
                    table.ForeignKey(
                        name: "FK_cuenta_financiera_negocio_id_negocio",
                        column: x => x.id_negocio,
                        principalSchema: "tanatos",
                        principalTable: "negocio",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_cuenta_financiera_tipo_cuenta_financiera_id_tipo_cuenta_fin~",
                        column: x => x.id_tipo_cuenta_financiera,
                        principalSchema: "tanatos",
                        principalTable: "tipo_cuenta_financiera",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene las cuentas financieras de un usuario.");

            migrationBuilder.CreateTable(
                name: "movimiento_financiero",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del movimiento financiero.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    id_cuenta_financiera = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador de la cuenta financiera a la que pertenece el movimiento."),
                    id_categoria_financiera = table.Column<long>(type: "bigint", nullable: true, comment: "Identificador de la categoría financiera a la que pertenece el movimiento."),
                    tipo_movimiento = table.Column<short>(type: "smallint", nullable: false, comment: "Tipo de movimiento - 1: Ingreso - 2: Gasto."),
                    monto = table.Column<decimal>(type: "numeric", nullable: false, comment: "Monto del movimiento."),
                    saldo = table.Column<decimal>(type: "numeric", nullable: false, comment: "Saldo resultante del movimiento."),
                    fecha_movimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se lleva a cabo el movimiento."),
                    descripcion = table.Column<string>(type: "text", nullable: true, comment: "Descripción del movimiento."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó el movimiento."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó el movimiento."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del movimiento.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimiento_financiero", x => x.id);
                    table.ForeignKey(
                        name: "FK_movimiento_financiero_categoria_financiera_id_categoria_fin~",
                        column: x => x.id_categoria_financiera,
                        principalSchema: "tanatos",
                        principalTable: "categoria_financiera",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_movimiento_financiero_cuenta_financiera_id_cuenta_financiera",
                        column: x => x.id_cuenta_financiera,
                        principalSchema: "tanatos",
                        principalTable: "cuenta_financiera",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene los movimientos de una cuenta financiera.");

            migrationBuilder.CreateIndex(
                name: "IX_categoria_financiera_id_negocio",
                schema: "tanatos",
                table: "categoria_financiera",
                column: "id_negocio");

            migrationBuilder.CreateIndex(
                name: "IX_cuenta_financiera_id_negocio",
                schema: "tanatos",
                table: "cuenta_financiera",
                column: "id_negocio");

            migrationBuilder.CreateIndex(
                name: "IX_cuenta_financiera_id_tipo_cuenta_financiera",
                schema: "tanatos",
                table: "cuenta_financiera",
                column: "id_tipo_cuenta_financiera");

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_financiero_id_categoria_financiera",
                schema: "tanatos",
                table: "movimiento_financiero",
                column: "id_categoria_financiera");

            migrationBuilder.CreateIndex(
                name: "IX_movimiento_financiero_id_cuenta_financiera_fecha_movimiento",
                schema: "tanatos",
                table: "movimiento_financiero",
                columns: new[] { "id_cuenta_financiera", "fecha_movimiento" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "movimiento_financiero",
                schema: "tanatos");

            migrationBuilder.DropTable(
                name: "categoria_financiera",
                schema: "tanatos");

            migrationBuilder.DropTable(
                name: "cuenta_financiera",
                schema: "tanatos");

            migrationBuilder.DropTable(
                name: "tipo_cuenta_financiera",
                schema: "tanatos");
        }
    }
}
