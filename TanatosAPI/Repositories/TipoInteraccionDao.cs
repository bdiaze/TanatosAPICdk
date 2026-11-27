using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Repositories {
	public class TipoInteraccionDao(IDatabaseConnectionHelper connectionHelper) : ITipoInteraccionDao {
		public async Task<TipoInteraccion?> Obtener(long id, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, NOMBRE, VIGENCIA FROM TANATOS.TIPO_INTERACCION WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("ID", id);

				await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

				TipoInteraccion? retorno = null;
				if (await reader.ReadAsync()) {
					retorno = new TipoInteraccion {
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

		public async Task<List<TipoInteraccion>> ObtenerTodos(NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, NOMBRE, VIGENCIA FROM TANATOS.TIPO_INTERACCION";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

				List<TipoInteraccion> retorno = [];
				while (await reader.ReadAsync()) {
					retorno.Add(new TipoInteraccion {
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

		public async Task Insertar(TipoInteraccion item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.TIPO_INTERACCION(ID, NOMBRE, VIGENCIA) " +
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

		public async Task Actualizar(TipoInteraccion item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.TIPO_INTERACCION SET NOMBRE = @NOMBRE, VIGENCIA = @VIGENCIA " +
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
				"DELETE FROM TANATOS.TIPO_INTERACCION WHERE ID = @ID";

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
