using Dapper;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class PagoDao(DatabaseConnectionHelper connectionHelper) {

		public async Task<Pago?> ObtenerPorFlow(long flowSubscriptionId, int flowPeriod) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return await connection.QueryFirstOrDefaultAsync<Pago>(
				"SELECT ID, SUB, ID_SUSCRIPCION, MONTO, MONEDA, FECHA_PAGO, ESTADO, FLOW_SUBSCRIPTION_ID, FLOW_PERIOD, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA " +
				"FROM TANATOS.PAGO WHERE FLOW_SUBSCRIPTION_ID = @FLOWSUBSCRIPTIONID AND FLOW_PERIOD = @FLOWPERIOD",
				new { flowSubscriptionId, flowPeriod }
			);
		}

		public async Task<long> Insertar(Pago item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.PAGO(SUB, ID_SUSCRIPCION, MONTO, MONEDA, FECHA_PAGO, ESTADO, FLOW_SUBSCRIPTION_ID, FLOW_PERIOD, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@SUB, @IDSUSCRIPCION, @MONTO, @MONEDA, @FECHAPAGO, @ESTADO, @FLOWSUBSCRIPTIONID, @FLOWPERIOD, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";
			DynamicParameters param = new();
			param.Add("SUB", item.Sub);
			param.Add("IDSUSCRIPCION", item.IdSuscripcion);
			param.Add("MONTO", item.Monto);
			param.Add("MONEDA", item.Moneda);
			param.Add("FECHAPAGO", item.FechaPago);
			param.Add("ESTADO", item.Estado);
			param.Add("FLOWSUBSCRIPTIONID", item.FlowSubscriptionId);
			param.Add("FLOWPERIOD", item.FlowPeriod);
			param.Add("FECHACREACION", item.FechaCreacion);
			param.Add("FECHAELIMINACION", item.FechaEliminacion);
			param.Add("VIGENCIA", item.Vigencia);

			if (transaction?.Connection != null) {
				return await transaction!.Connection!.ExecuteScalarAsync<long>(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				return await connection.ExecuteScalarAsync<long>(query, param);
			}
		}

		public async Task Actualizar(Pago item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.PAGO SET SUB = @SUB, ID_SUSCRIPCION = @IDSUSCRIPCION, MONTO = @MONTO, MONEDA = @MONEDA, " +
				"FECHA_PAGO = @FECHAPAGO, ESTADO = @ESTADO, FLOW_SUBSCRIPTION_ID = @FLOWSUBSCRIPTIONID, FLOW_PERIOD = @FLOWPERIOD, " +
				"FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";
			DynamicParameters param = new();
			param.Add("SUB", item.Sub);
			param.Add("IDSUSCRIPCION", item.IdSuscripcion);
			param.Add("MONTO", item.Monto);
			param.Add("MONEDA", item.Moneda);
			param.Add("FECHAPAGO", item.FechaPago);
			param.Add("ESTADO", item.Estado);
			param.Add("FLOWSUBSCRIPTIONID", item.FlowSubscriptionId);
			param.Add("FLOWPERIOD", item.FlowPeriod);
			param.Add("FECHACREACION", item.FechaCreacion);
			param.Add("FECHAELIMINACION", item.FechaEliminacion);
			param.Add("VIGENCIA", item.Vigencia);
			param.Add("ID", item.Id);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}
	}
}
