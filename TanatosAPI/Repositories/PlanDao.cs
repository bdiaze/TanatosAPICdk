using Dapper;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

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

		public async Task Insertar(Plan item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"INSERT INTO TANATOS.PLAN(ID, NOMBRE, PRECIO, DURACION_MESES, FLOW_PLAN_ID, VIGENCIA) VALUES (@ID, @NOMBRE, @PRECIO, @DURACIONMESES, @FLOWPLANID, @VIGENCIA)",
				new { item.Id, item.Nombre, item.Precio, item.DuracionMeses, item.FlowPlanId, item.Vigencia }
			);
		}

		public async Task Actualizar(Plan item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"UPDATE TANATOS.PLAN SET NOMBRE = @NOMBRE, PRECIO = @PRECIO, DURACION_MESES = @DURACIONMESES, FLOW_PLAN_ID = @FLOWPLANID, VIGENCIA = @VIGENCIA WHERE ID = @ID",
				new { item.Nombre, item.Precio, item.DuracionMeses, item.FlowPlanId, item.Vigencia, item.Id }
			);
		}

		public async Task Eliminar(long id) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"DELETE FROM TANATOS.PLAN WHERE ID = @ID",
				new { id }
			);
		}
	}
}
