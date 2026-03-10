using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Migrations
{
    /// <inheritdoc />
    public partial class ColumnFlowPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_pago_flow_payment_id",
                schema: "tanatos",
                table: "pago");

            migrationBuilder.DropColumn(
                name: "flow_payment_id",
                schema: "tanatos",
                table: "pago");

            migrationBuilder.AddColumn<int>(
                name: "flow_period",
                schema: "tanatos",
                table: "pago",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Periodo correspondiente al pago en la plataforma Flow.");

            migrationBuilder.AddColumn<long>(
                name: "flow_subscription_id",
                schema: "tanatos",
                table: "pago",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "ID de la suscripción en la plataforma Flow.");

            migrationBuilder.CreateIndex(
                name: "IX_pago_flow_subscription_id_flow_period",
                schema: "tanatos",
                table: "pago",
                columns: new[] { "flow_subscription_id", "flow_period" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_pago_flow_subscription_id_flow_period",
                schema: "tanatos",
                table: "pago");

            migrationBuilder.DropColumn(
                name: "flow_period",
                schema: "tanatos",
                table: "pago");

            migrationBuilder.DropColumn(
                name: "flow_subscription_id",
                schema: "tanatos",
                table: "pago");

            migrationBuilder.AddColumn<string>(
                name: "flow_payment_id",
                schema: "tanatos",
                table: "pago",
                type: "text",
                nullable: true,
                comment: "ID del pago en la plataforma Flow.");

            migrationBuilder.CreateIndex(
                name: "IX_pago_flow_payment_id",
                schema: "tanatos",
                table: "pago",
                column: "flow_payment_id",
                unique: true);
        }
    }
}
