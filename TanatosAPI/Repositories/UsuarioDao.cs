using Dapper;
using Npgsql;
using System.Data.Common;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class UsuarioDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<Usuario?> Obtener(string sub, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT SUB, FLOW_CUSTOMER_ID, NOMBRE, APELLIDO, CORREO_ELECTRONICO FROM TANATOS.USUARIO WHERE SUB = @SUB";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("SUB", sub);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				Usuario? retorno = null;
				if (await reader.ReadAsync()) {
					retorno = new Usuario {
						Sub = reader.GetString(0),
						FlowCustomerId = reader.IsDBNull(1) ? null : reader.GetString(1),
						Nombre = reader.IsDBNull(2) ? null : reader.GetString(2),
						Apellido = reader.IsDBNull(3) ? null : reader.GetString(3),
						CorreoElectronico = reader.IsDBNull(4) ? null : reader.GetString(4)
					};
				}

				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<Usuario?> ObtenerPorFlowCustomerId(string flowCustomerId) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return await connection.QueryFirstOrDefaultAsync<Usuario>(
				"SELECT SUB, FLOW_CUSTOMER_ID, NOMBRE, APELLIDO, CORREO_ELECTRONICO FROM TANATOS.USUARIO WHERE FLOW_CUSTOMER_ID = @FLOWCUSTOMERID",
				new { flowCustomerId }
			);
		}

		public async Task Insertar(Usuario item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.USUARIO(SUB, FLOW_CUSTOMER_ID, NOMBRE, APELLIDO, CORREO_ELECTRONICO) " +
				"VALUES (@SUB, @FLOWCUSTOMERID, @NOMBRE, @APELLIDO, @CORREOELECTRONICO)";
			DynamicParameters param = new();
			param.Add("SUB", item.Sub);
			param.Add("FLOWCUSTOMERID", item.FlowCustomerId);
			param.Add("NOMBRE", item.Nombre);
			param.Add("APELLIDO", item.Apellido);
			param.Add("CORREOELECTRONICO", item.CorreoElectronico);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}

		public async Task Actualizar(Usuario item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.USUARIO SET FLOW_CUSTOMER_ID = @FLOWCUSTOMERID, NOMBRE = @NOMBRE, APELLIDO = @APELLIDO, CORREO_ELECTRONICO = @CORREOELECTRONICO " +
				"WHERE SUB = @SUB";
			DynamicParameters param = new();
			param.Add("FLOWCUSTOMERID", item.FlowCustomerId);
			param.Add("NOMBRE", item.Nombre);
			param.Add("APELLIDO", item.Apellido);
			param.Add("CORREOELECTRONICO", item.CorreoElectronico);
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
