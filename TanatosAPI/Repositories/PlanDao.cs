using Dapper;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using static Google.Cloud.RecaptchaEnterprise.V1.TransactionData.Types;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class PlanDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<List<Plan>> ObtenerPorVigencia(bool? vigencia = true) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<Plan>(
				"SELECT ID, NOMBRE, PRECIO, DURACION_MESES, FLOW_PLAN_ID, VIGENCIA FROM TANATOS.PLAN WHERE (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)",
				new { vigencia }
			)];
		}

		public async Task Insertar(Plan item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.PLAN(ID, NOMBRE, PRECIO, DURACION_MESES, FLOW_PLAN_ID, VIGENCIA) VALUES (@ID, @NOMBRE, @PRECIO, @DURACIONMESES, @FLOWPLANID, @VIGENCIA)";
			DynamicParameters param = new();
			param.Add("ID", item.Id);
			param.Add("NOMBRE", item.Nombre);
			param.Add("PRECIO", item.Precio);
			param.Add("DURACIONMESES", item.DuracionMeses);
			param.Add("FLOWPLANID", item.FlowPlanId);
			param.Add("VIGENCIA", item.Vigencia);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}

		public async Task Actualizar(Plan item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.PLAN SET NOMBRE = @NOMBRE, PRECIO = @PRECIO, DURACION_MESES = @DURACIONMESES, FLOW_PLAN_ID = @FLOWPLANID, VIGENCIA = @VIGENCIA WHERE ID = @ID";
			DynamicParameters param = new();
			param.Add("NOMBRE", item.Nombre);
			param.Add("PRECIO", item.Precio);
			param.Add("DURACIONMESES", item.DuracionMeses);
			param.Add("FLOWPLANID", item.FlowPlanId);
			param.Add("VIGENCIA", item.Vigencia);
			param.Add("ID", item.Id);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}

		public async Task Eliminar(long id, NpgsqlTransaction? transaction = null) {
			string query =
				"DELETE FROM TANATOS.PLAN WHERE ID = @ID";
			DynamicParameters param = new();
			param.Add("ID", id);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}
	}
}
