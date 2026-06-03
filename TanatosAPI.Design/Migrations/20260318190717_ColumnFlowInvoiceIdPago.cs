using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TanatosAPI.Design.Migrations
{
    /// <inheritdoc />
    public partial class ColumnFlowInvoiceIdPago : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_pago_flow_subscription_id_flow_period",
                schema: "tanatos",
                table: "pago");

            migrationBuilder.DropColumn(
                name: "flow_period",
                schema: "tanatos",
                table: "pago");

            migrationBuilder.AlterColumn<string>(
                name: "flow_subscription_id",
                schema: "tanatos",
                table: "pago",
                type: "text",
                nullable: false,
                comment: "ID de la suscripción en la plataforma Flow.",
                oldClrType: typeof(long),
                oldType: "bigint",
                oldComment: "ID de la suscripción en la plataforma Flow.");

            migrationBuilder.AddColumn<string>(
                name: "flow_invoice_id",
                schema: "tanatos",
                table: "pago",
                type: "text",
                nullable: false,
                defaultValue: "",
                comment: "ID del invoice en la plataforma Flow.");

            migrationBuilder.CreateIndex(
                name: "IX_pago_flow_subscription_id_flow_invoice_id",
                schema: "tanatos",
                table: "pago",
                columns: new[] { "flow_subscription_id", "flow_invoice_id" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_pago_flow_subscription_id_flow_invoice_id",
                schema: "tanatos",
                table: "pago");

            migrationBuilder.DropColumn(
                name: "flow_invoice_id",
                schema: "tanatos",
                table: "pago");

            migrationBuilder.AlterColumn<long>(
                name: "flow_subscription_id",
                schema: "tanatos",
                table: "pago",
                type: "bigint",
                nullable: false,
                comment: "ID de la suscripción en la plataforma Flow.",
                oldClrType: typeof(string),
                oldType: "text",
                oldComment: "ID de la suscripción en la plataforma Flow.");

            migrationBuilder.AddColumn<int>(
                name: "flow_period",
                schema: "tanatos",
                table: "pago",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "Periodo correspondiente al pago en la plataforma Flow.");

            migrationBuilder.CreateIndex(
                name: "IX_pago_flow_subscription_id_flow_period",
                schema: "tanatos",
                table: "pago",
                columns: new[] { "flow_subscription_id", "flow_period" },
                unique: true);
        }
    }
}
