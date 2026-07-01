using Npgsql;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Repositories {
	[ExcludeFromCodeCoverage]
	public class VideoTutorialDao(IDatabaseConnectionHelper connectionHelper) : IVideoTutorialDao {
		public async Task<List<VideoTutorial>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, TITULO, DESCRIPCION, URL, HABILITADO, ORDEN, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA " +
				"FROM TANATOS.VIDEO_TUTORIAL WHERE (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				List<VideoTutorial> retorno = [];
				while (await reader.ReadAsync()) {
					retorno.Add(new VideoTutorial {
						Id = reader.GetInt64(0),
						Titulo = reader.GetString(1),
						Descripcion = await reader.IsDBNullAsync(2) ? null : reader.GetString(2),
						Url = reader.GetString(3),
						Habilitado = reader.GetBoolean(4),
						Orden = reader.GetInt32(5),
						FechaCreacion = reader.GetDateTime(6),
						FechaEliminacion = await reader.IsDBNullAsync(7) ? null : reader.GetDateTime(7),
						Vigencia = reader.GetBoolean(8)
					});
				}
				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<long> Insertar(VideoTutorial item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.VIDEO_TUTORIAL(TITULO, DESCRIPCION, URL, HABILITADO, ORDEN, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@TITULO, @DESCRIPCION, @URL, @HABILITADO, @ORDEN, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);

				command.Parameters.AddWithValue("TITULO", item.Titulo);
				command.Parameters.AddWithValue("DESCRIPCION", (object?)item.Descripcion ?? DBNull.Value);
				command.Parameters.AddWithValue("URL", item.Url);
				command.Parameters.AddWithValue("HABILITADO", item.Habilitado);
				command.Parameters.AddWithValue("ORDEN", item.Orden);
				command.Parameters.AddWithValue("FECHACREACION", item.FechaCreacion);
				command.Parameters.AddWithValue("FECHAELIMINACION", (object?)item.FechaEliminacion ?? DBNull.Value);
				command.Parameters.AddWithValue("VIGENCIA", item.Vigencia);

				return Convert.ToInt64(await command.ExecuteScalarAsync());
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task Actualizar(VideoTutorial item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.VIDEO_TUTORIAL SET TITULO = @TITULO, DESCRIPCION = @DESCRIPCION, URL = @URL, HABILITADO = @HABILITADO, ORDEN = @ORDEN, " +
				"FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);

				command.Parameters.AddWithValue("TITULO", item.Titulo);
				command.Parameters.AddWithValue("DESCRIPCION", (object?)item.Descripcion ?? DBNull.Value);
				command.Parameters.AddWithValue("URL", item.Url);
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
