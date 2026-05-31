using Dapper;
using Npgsql;
using System.Data.Common;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class SuscripcionDao(DatabaseConnectionHelper connectionHelper) {

		public async Task<Suscripcion?> Obtener(long id) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return await connection.QueryFirstOrDefaultAsync<Suscripcion>(
				"SELECT ID, SUB, ID_PLAN, FECHA_INICIO, FECHA_EXPIRACION, FECHA_CANCELACION, ESTADO, FLOW_CUSTOMER_ID, FLOW_SUBSCRIPTION_ID, " +
				"FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.SUSCRIPCION WHERE ID = @ID",
				new { id }
			);
		}

		public async Task<Suscripcion?> ObtenerPorFlowSubscriptionId(string flowSubscriptionId) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return await connection.QueryFirstOrDefaultAsync<Suscripcion>(
				"SELECT ID, SUB, ID_PLAN, FECHA_INICIO, FECHA_EXPIRACION, FECHA_CANCELACION, ESTADO, FLOW_CUSTOMER_ID, FLOW_SUBSCRIPTION_ID, " +
				"FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.SUSCRIPCION WHERE FLOW_SUBSCRIPTION_ID = @FLOWSUBSCRIPTIONID",
				new { flowSubscriptionId }
			);
		}

		public async Task<List<Suscripcion>> ObtenerPorSub(string sub, bool? vigencia = true, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, SUB, ID_PLAN, FECHA_INICIO, FECHA_EXPIRACION, FECHA_CANCELACION, ESTADO, FLOW_CUSTOMER_ID, FLOW_SUBSCRIPTION_ID, " +
				"FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.SUSCRIPCION WHERE SUB = @SUB AND (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("SUB", sub);
				command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				List<Suscripcion> retorno = [];

				while (await reader.ReadAsync()) {
					retorno.Add(new Suscripcion {
						Id = reader.GetInt64(0),
						Sub = reader.GetString(1),
						IdPlan = reader.GetInt64(2),
						FechaInicio = await reader.IsDBNullAsync(3) ? null : reader.GetDateTime(3),
						FechaExpiracion = await reader.IsDBNullAsync(4) ? null : reader.GetDateTime(4),
						FechaCancelacion = await reader.IsDBNullAsync(5) ? null : reader.GetDateTime(5),
						Estado = reader.GetInt16(6),
						FlowCustomerId = await reader.IsDBNullAsync(7) ? null : reader.GetString(7),
						FlowSubscriptionId = await reader.IsDBNullAsync(8) ? null : reader.GetString(8),
						FechaCreacion = reader.GetDateTime(9),
						FechaEliminacion = await reader.IsDBNullAsync(10) ? null : reader.GetDateTime(10),
						Vigencia = reader.GetBoolean(11)
					});
				}

				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<long> Insertar(Suscripcion item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.SUSCRIPCION(SUB, ID_PLAN, FECHA_INICIO, FECHA_EXPIRACION, FECHA_CANCELACION, ESTADO, FLOW_CUSTOMER_ID, FLOW_SUBSCRIPTION_ID, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@SUB, @IDPLAN, @FECHAINICIO, @FECHAEXPIRACION, @FECHACANCELACION, @ESTADO, @FLOWCUSTOMERID, @FLOWSUBSCRIPTIONID, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";
			DynamicParameters param = new();
			param.Add("SUB", item.Sub);
			param.Add("IDPLAN", item.IdPlan);
			param.Add("FECHAINICIO", item.FechaInicio);
			param.Add("FECHAEXPIRACION", item.FechaExpiracion);
			param.Add("FECHACANCELACION", item.FechaCancelacion);
			param.Add("ESTADO", item.Estado);
			param.Add("FLOWCUSTOMERID", item.FlowCustomerId);
			param.Add("FLOWSUBSCRIPTIONID", item.FlowSubscriptionId);
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

		public async Task Actualizar(Suscripcion item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.SUSCRIPCION SET SUB = @SUB, ID_PLAN = @IDPLAN, FECHA_INICIO = @FECHAINICIO, FECHA_EXPIRACION = @FECHAEXPIRACION, " +
				"FECHA_CANCELACION = @FECHACANCELACION, ESTADO = @ESTADO, FLOW_CUSTOMER_ID = @FLOWCUSTOMERID, FLOW_SUBSCRIPTION_ID = @FLOWSUBSCRIPTIONID, " +
				"FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";
			DynamicParameters param = new();
			param.Add("SUB", item.Sub);
			param.Add("IDPLAN", item.IdPlan);
			param.Add("FECHAINICIO", item.FechaInicio);
			param.Add("FECHAEXPIRACION", item.FechaExpiracion);
			param.Add("FECHACANCELACION", item.FechaCancelacion);
			param.Add("ESTADO", item.Estado);
			param.Add("FLOWCUSTOMERID", item.FlowCustomerId);
			param.Add("FLOWSUBSCRIPTIONID", item.FlowSubscriptionId);
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
