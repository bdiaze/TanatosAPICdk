using Npgsql;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Repositories {
	[ExcludeFromCodeCoverage]
	public class TipoProcesoAutomaticoDao(IDatabaseConnectionHelper connectionHelper) : ITipoProcesoAutomaticoDao {
		public async Task<TipoProcesoAutomatico?> Obtener(long id, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, NOMBRE, DESCRIPCION, HABILITADO, ORDEN, FECHA_CREACION " +
				"FROM TANATOS.TIPO_PROCESO_AUTOMATICO WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("ID", id);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				TipoProcesoAutomatico? retorno = null;
				if (await reader.ReadAsync()) {
					retorno = new TipoProcesoAutomatico {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						Nombre = reader.GetString(reader.GetOrdinal("NOMBRE")),
						Descripcion = await reader.IsDBNullAsync(reader.GetOrdinal("DESCRIPCION")) ? null : reader.GetString(reader.GetOrdinal("DESCRIPCION")),
						Habilitado = reader.GetBoolean(reader.GetOrdinal("HABILITADO")),
						Orden = reader.GetInt32(reader.GetOrdinal("ORDEN")),
						FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FECHA_CREACION")),
					};
				}
				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<List<TipoProcesoAutomatico>> ObtenerTodos(NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, NOMBRE, DESCRIPCION, HABILITADO, ORDEN, FECHA_CREACION " +
				"FROM TANATOS.TIPO_PROCESO_AUTOMATICO";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				List<TipoProcesoAutomatico> retorno = [];
				while (await reader.ReadAsync()) {
					retorno.Add(new TipoProcesoAutomatico {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						Nombre = reader.GetString(reader.GetOrdinal("NOMBRE")),
						Descripcion = await reader.IsDBNullAsync(reader.GetOrdinal("DESCRIPCION")) ? null : reader.GetString(reader.GetOrdinal("DESCRIPCION")),
						Habilitado = reader.GetBoolean(reader.GetOrdinal("HABILITADO")),
						Orden = reader.GetInt32(reader.GetOrdinal("ORDEN")),
						FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FECHA_CREACION")),
					});
				}
				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task Insertar(TipoProcesoAutomatico item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.TIPO_PROCESO_AUTOMATICO(ID, NOMBRE, DESCRIPCION, HABILITADO, ORDEN, FECHA_CREACION) " +
				"VALUES (@ID, @NOMBRE, @DESCRIPCION, @HABILITADO, @ORDEN, @FECHACREACION)";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);

				command.Parameters.AddWithValue("ID", item.Id);
				command.Parameters.AddWithValue("NOMBRE", item.Nombre);
				command.Parameters.AddWithValue("DESCRIPCION", (object?)item.Descripcion ?? DBNull.Value);
				command.Parameters.AddWithValue("HABILITADO", item.Habilitado);
				command.Parameters.AddWithValue("ORDEN", item.Orden);
				command.Parameters.AddWithValue("FECHACREACION", item.FechaCreacion);

				await command.ExecuteNonQueryAsync();
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task Actualizar(TipoProcesoAutomatico item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.TIPO_PROCESO_AUTOMATICO SET NOMBRE = @NOMBRE, DESCRIPCION = @DESCRIPCION, HABILITADO = @HABILITADO, ORDEN = @ORDEN, " +
				"FECHA_CREACION = @FECHACREACION " +
				"WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);

				command.Parameters.AddWithValue("NOMBRE", item.Nombre);
				command.Parameters.AddWithValue("DESCRIPCION", (object?)item.Descripcion ?? DBNull.Value);
				command.Parameters.AddWithValue("HABILITADO", item.Habilitado);
				command.Parameters.AddWithValue("ORDEN", item.Orden);
				command.Parameters.AddWithValue("FECHACREACION", item.FechaCreacion);
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
				"DELETE FROM TANATOS.TIPO_PROCESO_AUTOMATICO WHERE ID = @ID";

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
