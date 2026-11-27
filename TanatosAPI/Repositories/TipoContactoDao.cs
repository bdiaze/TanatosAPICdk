using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Repositories {
	public class TipoContactoDao(IDatabaseConnectionHelper connectionHelper) : ITipoContactoDao {
		public async Task<TipoContacto?> Obtener(long id, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, NOMBRE, REGEX_VALIDACION, VIGENCIA FROM TANATOS.TIPO_CONTACTO WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("ID", id);

				await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

				TipoContacto? retorno = null;
				if (await reader.ReadAsync()) {
					retorno = new TipoContacto {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						Nombre = reader.GetString(reader.GetOrdinal("NOMBRE")),
						RegexValidacion = reader.GetString(reader.GetOrdinal("REGEX_VALIDACION")),
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

		public async Task<List<TipoContacto>> ObtenerTodos(NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, NOMBRE, REGEX_VALIDACION, VIGENCIA FROM TANATOS.TIPO_CONTACTO";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

				List<TipoContacto> retorno = [];
				while (await reader.ReadAsync()) {
					retorno.Add(new TipoContacto {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						Nombre = reader.GetString(reader.GetOrdinal("NOMBRE")),
						RegexValidacion = reader.GetString(reader.GetOrdinal("REGEX_VALIDACION")),
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

		public async Task Insertar(TipoContacto item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.TIPO_CONTACTO(ID, NOMBRE, REGEX_VALIDACION, VIGENCIA) " +
				"VALUES (@ID, @NOMBRE, @REGEXVALIDACION, @VIGENCIA)";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("ID", item.Id);
				command.Parameters.AddWithValue("NOMBRE", item.Nombre);
				command.Parameters.AddWithValue("REGEXVALIDACION", (object?)item.RegexValidacion ?? DBNull.Value);
				command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);
				await command.ExecuteNonQueryAsync();
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task Actualizar(TipoContacto item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.TIPO_CONTACTO SET NOMBRE = @NOMBRE, REGEX_VALIDACION = @REGEXVALIDACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("NOMBRE", item.Nombre);
				command.Parameters.AddWithValue("REGEXVALIDACION", (object?)item.RegexValidacion ?? DBNull.Value);
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
				"DELETE FROM TANATOS.TIPO_CONTACTO WHERE ID = @ID";

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
