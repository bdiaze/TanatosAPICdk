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
				"SELECT ID, NOMBRE, DESCRIPCION, HABILITADO, ORDEN, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA " +
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
						Id = reader.GetInt64(0),
						Nombre = reader.GetString(1),
						Descripcion = await reader.IsDBNullAsync(2) ? null : reader.GetString(2),
						Habilitado = reader.GetBoolean(3),
						Orden = reader.GetInt32(4),
						FechaCreacion = reader.GetDateTime(5),
						FechaEliminacion = await reader.IsDBNullAsync(6) ? null : reader.GetDateTime(6),
						Vigencia = reader.GetBoolean(7)
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
				"SELECT ID, NOMBRE, DESCRIPCION, HABILITADO, ORDEN, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA " +
				"FROM TANATOS.TIPO_PROCESO_AUTOMATICO";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				List<TipoProcesoAutomatico> retorno = [];
				while (await reader.ReadAsync()) {
					retorno.Add(new TipoProcesoAutomatico {
						Id = reader.GetInt64(0),
						Nombre = reader.GetString(1),
						Descripcion = await reader.IsDBNullAsync(2) ? null : reader.GetString(2),
						Habilitado = reader.GetBoolean(3),
						Orden = reader.GetInt32(4),
						FechaCreacion = reader.GetDateTime(5),
						FechaEliminacion = await reader.IsDBNullAsync(6) ? null : reader.GetDateTime(6),
						Vigencia = reader.GetBoolean(7)
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
				"INSERT INTO TANATOS.TIPO_PROCESO_AUTOMATICO(ID, NOMBRE, DESCRIPCION, HABILITADO, ORDEN, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@ID, @NOMBRE, @DESCRIPCION, @HABILITADO, @ORDEN, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA)";

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
				command.Parameters.AddWithValue("FECHAELIMINACION", (object?)item.FechaEliminacion ?? DBNull.Value);
				command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);

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
				"FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
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
				command.Parameters.AddWithValue("FECHAELIMINACION", (object?)item.FechaEliminacion ?? DBNull.Value);
				command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);
				command.Parameters.AddWithValue("ID", item.Id);

				await command.ExecuteNonQueryAsync();
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}
	}
}
