using Dapper;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class UsuarioDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<Usuario?> Obtener(string sub) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return await connection.QueryFirstOrDefaultAsync<Usuario>(
				"SELECT SUB, FLOW_CUSTOMER_ID FROM TANATOS.USUARIO WHERE SUB = @SUB",
				new { sub }
			);
		}

		public async Task<Usuario?> ObtenerPorFlowCustomerId(string flowCustomerId) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return await connection.QueryFirstOrDefaultAsync<Usuario>(
				"SELECT SUB, FLOW_CUSTOMER_ID FROM TANATOS.USUARIO WHERE FLOW_CUSTOMER_ID = @FLOWCUSTOMERID",
				new { flowCustomerId }
			);
		}

		public async Task Insertar(Usuario item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.USUARIO(SUB, FLOW_CUSTOMER_ID) VALUES (@SUB, @FLOWCUSTOMERID)";
			DynamicParameters param = new();
			param.Add("SUB", item.Sub);
			param.Add("FLOWCUSTOMERID", item.FlowCustomerId);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}

		public async Task Actualizar(Usuario item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.USUARIO SET FLOW_CUSTOMER_ID = @FLOWCUSTOMERID WHERE SUB = @SUB";
			DynamicParameters param = new();
			param.Add("FLOWCUSTOMERID", item.FlowCustomerId);
			param.Add("SUB", item.Sub);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}
	}
}
