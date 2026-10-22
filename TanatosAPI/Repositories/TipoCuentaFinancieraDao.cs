using Npgsql;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Repositories {
	[ExcludeFromCodeCoverage]
	public class TipoCuentaFinancieraDao(IDatabaseConnectionHelper connectionHelper) : ITipoCuentaFinancieraDao {
		public async Task<TipoCuentaFinanciera?> Obtener(long id, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, NOMBRE, VIGENCIA FROM TANATOS.TIPO_CUENTA_FINANCIERA WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("ID", id);

				await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

				TipoCuentaFinanciera? retorno = null;
				if (await reader.ReadAsync()) {
					retorno = new TipoCuentaFinanciera {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						Nombre = reader.GetString(reader.GetOrdinal("NOMBRE")),
						Vigencia = reader.GetBoolean(reader.GetOrdinal("VIGENCIA")),
					};
				}
				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<List<TipoCuentaFinanciera>> ObtenerTodos(NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, NOMBRE, VIGENCIA FROM TANATOS.TIPO_CUENTA_FINANCIERA";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

				List<TipoCuentaFinanciera> retorno = [];
				while (await reader.ReadAsync()) {
					retorno.Add(new TipoCuentaFinanciera {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						Nombre = reader.GetString(reader.GetOrdinal("NOMBRE")),
						Vigencia = reader.GetBoolean(reader.GetOrdinal("VIGENCIA")),
					});
				}
				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task Insertar(TipoCuentaFinanciera item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.TIPO_CUENTA_FINANCIERA(ID, NOMBRE, VIGENCIA) " +
				"VALUES (@ID, @NOMBRE, @VIGENCIA)";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("ID", item.Id);
				command.Parameters.AddWithValue("NOMBRE", item.Nombre);
				command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);
				await command.ExecuteNonQueryAsync();
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task Actualizar(TipoCuentaFinanciera item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.TIPO_CUENTA_FINANCIERA SET NOMBRE = @NOMBRE, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("NOMBRE", item.Nombre);
				command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);
				command.Parameters.AddWithValue("ID", item.Id);
				await command.ExecuteNonQueryAsync();
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task Eliminar(long id, NpgsqlTransaction? transaction = null) {
			string query =
				"DELETE FROM TANATOS.TIPO_CUENTA_FINANCIERA WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("ID", id);
				await command.ExecuteNonQueryAsync();
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}
	}
}
