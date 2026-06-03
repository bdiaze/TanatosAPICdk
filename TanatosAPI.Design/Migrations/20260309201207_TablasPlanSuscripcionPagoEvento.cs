using System;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class TablasPlanSuscripcionPagoEvento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "evento_pago",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del evento de pago.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    proveedor = table.Column<string>(type: "text", nullable: false, comment: "Proveedor que informa el evento de pago."),
                    evento = table.Column<string>(type: "text", nullable: false, comment: "Tipo de evento."),
                    payload = table.Column<JsonDocument>(type: "jsonb", nullable: false, comment: "Payload del evento recepcionado desde el proveedor."),
                    procesado = table.Column<bool>(type: "boolean", nullable: false, comment: "Indicador de si evento fue procesado."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó el evento de pago."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó el evento de pago."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del evento de pago.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_evento_pago", x => x.id);
                },
                comment: "Tabla que contiene los eventos de pagos recepcionados.");

            migrationBuilder.CreateTable(
                name: "plan",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del plan."),
                    nombre = table.Column<string>(type: "text", nullable: false, comment: "Nombre del plan."),
                    precio_mensual = table.Column<decimal>(type: "numeric", nullable: false, comment: "Precio mensual del plan."),
                    precio_anual = table.Column<decimal>(type: "numeric", nullable: true, comment: "Precio anual del plan."),
                    duracion_meses = table.Column<int>(type: "integer", nullable: false, comment: "Duración del plan en meses."),
                    flow_plan_id = table.Column<string>(type: "text", nullable: true, comment: "ID del plan en la plataforma Flow."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del plan.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plan", x => x.id);
                },
                comment: "Tabla que contiene los planes de suscripción.");

            migrationBuilder.CreateTable(
                name: "suscripcion",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador de la suscripción.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sub = table.Column<string>(type: "text", nullable: false, comment: "Usuario al que pertenece la suscripción."),
                    id_plan = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del plan al que el usuario está suscrito."),
                    fecha_inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se inició la suscripción."),
                    fecha_expiracion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que expira la suscripción."),
                    fecha_cancelacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se cancela la suscripción."),
                    estado = table.Column<short>(type: "smallint", nullable: false, comment: "Estado de la suscripción. 1: Activa - 2: Cancelada - 3: Expirada - 4: Pago Pendiente."),
                    flow_customer_id = table.Column<string>(type: "text", nullable: true, comment: "ID del cliente en la plataforma Flow."),
                    flow_subscription_id = table.Column<string>(type: "text", nullable: true, comment: "ID de la suscripción en la plataforma Flow."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó la suscripción."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó la suscripción."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia de la suscripción.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suscripcion", x => x.id);
                    table.ForeignKey(
                        name: "FK_suscripcion_plan_id_plan",
                        column: x => x.id_plan,
                        principalSchema: "tanatos",
                        principalTable: "plan",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene las suscripciones de los usuarios.");

            migrationBuilder.CreateTable(
                name: "pago",
                schema: "tanatos",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador del pago.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sub = table.Column<string>(type: "text", nullable: false, comment: "Usuario al que pertenece el pago."),
                    id_suscripcion = table.Column<long>(type: "bigint", nullable: false, comment: "Identificador de la suscripción a la que pertenece el pago."),
                    monto = table.Column<decimal>(type: "numeric", nullable: false, comment: "Monto del pago efectuado."),
                    moneda = table.Column<string>(type: "text", nullable: false, comment: "Moneda en que se efectuó el pago."),
                    fecha_pago = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se efectuó el pago."),
                    estado = table.Column<short>(type: "smallint", nullable: false, comment: "Estado del pago. 0: Pendiente - 1: Pagado - 2: Fallido - 3: Reembolsado."),
                    flow_payment_id = table.Column<string>(type: "text", nullable: true, comment: "ID del pago en la plataforma Flow."),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "Fecha en que se creó el pago."),
                    fecha_eliminacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "Fecha en que se eliminó el pago."),
                    vigencia = table.Column<bool>(type: "boolean", nullable: false, comment: "Vigencia del pago.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pago", x => x.id);
                    table.ForeignKey(
                        name: "FK_pago_suscripcion_id_suscripcion",
                        column: x => x.id_suscripcion,
                        principalSchema: "tanatos",
                        principalTable: "suscripcion",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                },
                comment: "Tabla que contiene los pagos de los usuarios.");

            migrationBuilder.CreateIndex(
                name: "IX_pago_flow_payment_id",
                schema: "tanatos",
                table: "pago",
                column: "flow_payment_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pago_id_suscripcion",
                schema: "tanatos",
                table: "pago",
                column: "id_suscripcion");

            migrationBuilder.CreateIndex(
                name: "IX_pago_sub",
                schema: "tanatos",
                table: "pago",
                column: "sub");

            migrationBuilder.CreateIndex(
                name: "IX_plan_flow_plan_id",
                schema: "tanatos",
                table: "plan",
                column: "flow_plan_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_suscripcion_flow_subscription_id",
                schema: "tanatos",
                table: "suscripcion",
                column: "flow_subscription_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_suscripcion_id_plan",
                schema: "tanatos",
                table: "suscripcion",
                column: "id_plan");

            migrationBuilder.CreateIndex(
                name: "IX_suscripcion_sub",
                schema: "tanatos",
                table: "suscripcion",
                column: "sub");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "evento_pago",
                schema: "tanatos");

            migrationBuilder.DropTable(
                name: "pago",
                schema: "tanatos");

            migrationBuilder.DropTable(
                name: "suscripcion",
                schema: "tanatos");

            migrationBuilder.DropTable(
                name: "plan",
                schema: "tanatos");
        }
    }
}
