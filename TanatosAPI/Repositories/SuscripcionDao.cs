using Dapper;
using Npgsql;
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

		public async Task<List<Suscripcion>> ObtenerPorSub(string sub, bool? vigencia = true) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<Suscripcion>(
				"SELECT ID, SUB, ID_PLAN, FECHA_INICIO, FECHA_EXPIRACION, FECHA_CANCELACION, ESTADO, FLOW_CUSTOMER_ID, FLOW_SUBSCRIPTION_ID, " +
				"FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.SUSCRIPCION WHERE SUB = @SUB AND (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)",
				new { sub, vigencia }
			)];
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
