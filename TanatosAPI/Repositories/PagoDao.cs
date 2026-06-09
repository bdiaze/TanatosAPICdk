using Npgsql;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Repositories {
    [ExcludeFromCodeCoverage]
    public class PagoDao(IDatabaseConnectionHelper connectionHelper) {

		public async Task<Pago?> ObtenerPorFlow(string flowSubscriptionId, string flowInvoiceId, NpgsqlTransaction? transaction = null) {
            string query =
                "SELECT ID, SUB, ID_SUSCRIPCION, MONTO, MONEDA, FECHA_PAGO, ESTADO, FLOW_SUBSCRIPTION_ID, " +
				"FLOW_INVOICE_ID, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA " +
                "FROM TANATOS.PAGO WHERE FLOW_SUBSCRIPTION_ID = @FLOWSUBSCRIPTIONID AND FLOW_INVOICE_ID = @FLOWINVOICEID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);

                command.Parameters.AddWithValue("FLOWSUBSCRIPTIONID", flowSubscriptionId);
                command.Parameters.AddWithValue("FLOWINVOICEID", flowInvoiceId);

                await using DbDataReader reader = await command.ExecuteReaderAsync();

                Pago? retorno = null;
                if (await reader.ReadAsync()) {
                    retorno = new Pago {
                        Id = reader.GetInt64(0),
						Sub = reader.GetString(1),
						IdSuscripcion = reader.GetInt64(2),
						Monto = reader.GetDecimal(3),
						Moneda = reader.GetString(4),
						FechaPago = await reader.IsDBNullAsync(5) ? null : reader.GetDateTime(5),
						Estado = reader.GetInt16(6),
						FlowSubscriptionId = reader.GetString(7),
						FlowInvoiceId = reader.GetString(8),
						FechaCreacion = reader.GetDateTime(9),
						FechaEliminacion = await reader.IsDBNullAsync(10) ? null : reader.GetDateTime(10),
						Vigencia = reader.GetBoolean(11)
                    };
                }
                return retorno;
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
        }

		public async Task<long> Insertar(Pago item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.PAGO(SUB, ID_SUSCRIPCION, MONTO, MONEDA, FECHA_PAGO, ESTADO, FLOW_SUBSCRIPTION_ID, FLOW_INVOICE_ID, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@SUB, @IDSUSCRIPCION, @MONTO, @MONEDA, @FECHAPAGO, @ESTADO, @FLOWSUBSCRIPTIONID, @FLOWINVOICEID, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("SUB", item.Sub);
                command.Parameters.AddWithValue("IDSUSCRIPCION", item.IdSuscripcion);
                command.Parameters.AddWithValue("MONTO", item.Monto);
                command.Parameters.AddWithValue("MONEDA", item.Moneda);
                command.Parameters.AddWithValue("FECHAPAGO", (object?)item.FechaPago ?? DBNull.Value);
                command.Parameters.AddWithValue("ESTADO", item.Estado);
                command.Parameters.AddWithValue("FLOWSUBSCRIPTIONID", item.FlowSubscriptionId);
                command.Parameters.AddWithValue("FLOWINVOICEID", item.FlowInvoiceId);
                command.Parameters.AddWithValue("FECHACREACION", item.FechaCreacion);
                command.Parameters.AddWithValue("FECHAELIMINACION", (object?)item.FechaEliminacion ?? DBNull.Value);
                command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);
                return Convert.ToInt64(await command.ExecuteScalarAsync());
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
		}

		public async Task Actualizar(Pago item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.PAGO SET SUB = @SUB, ID_SUSCRIPCION = @IDSUSCRIPCION, MONTO = @MONTO, MONEDA = @MONEDA, " +
				"FECHA_PAGO = @FECHAPAGO, ESTADO = @ESTADO, FLOW_SUBSCRIPTION_ID = @FLOWSUBSCRIPTIONID, FLOW_INVOICE_ID = @FLOWINVOICEID, " +
				"FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("SUB", item.Sub);
                command.Parameters.AddWithValue("IDSUSCRIPCION", item.IdSuscripcion);
                command.Parameters.AddWithValue("MONTO", item.Monto);
                command.Parameters.AddWithValue("MONEDA", item.Moneda);
                command.Parameters.AddWithValue("FECHAPAGO", (object?)item.FechaPago ?? DBNull.Value);
                command.Parameters.AddWithValue("ESTADO", item.Estado);
                command.Parameters.AddWithValue("FLOWSUBSCRIPTIONID", item.FlowSubscriptionId);
                command.Parameters.AddWithValue("FLOWINVOICEID", item.FlowInvoiceId);
                command.Parameters.AddWithValue("FECHACREACION", item.FechaCreacion);
                command.Parameters.AddWithValue("FECHAELIMINACION", (object?)item.FechaEliminacion ?? DBNull.Value);
                command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);
                command.Parameters.AddWithValue("ID", item.Id);
                await command.ExecuteNonQueryAsync();
            } finally {
                if (disposeConnection && connection != null) {
                    await connection.DisposeAsync();
                }
            }
		}
	}
}
