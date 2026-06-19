using Npgsql;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces;

namespace TanatosAPI.Repositories {
	[ExcludeFromCodeCoverage]
	public class PreguntaFrecuenteDao(IDatabaseConnectionHelper connectionHelper) : IPreguntaFrecuenteDao {
		public async Task<List<PreguntaFrecuente>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, PREGUNTA, RESPUESTA, HABILITADO, ORDEN, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA " +
				"FROM TANATOS.PREGUNTA_FRECUENTE WHERE (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				List<PreguntaFrecuente> retorno = [];
				while (await reader.ReadAsync()) {
					retorno.Add(new PreguntaFrecuente {
						Id = reader.GetInt64(0),
						Pregunta = reader.GetString(1),
						Respuesta = reader.GetString(2),
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

		public async Task<long> Insertar(PreguntaFrecuente item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.PREGUNTA_FRECUENTE(PREGUNTA, RESPUESTA, HABILITADO, ORDEN, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@PREGUNTA, @RESPUESTA, @HABILITADO, @ORDEN, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);

				command.Parameters.AddWithValue("PREGUNTA", item.Pregunta);
				command.Parameters.AddWithValue("RESPUESTA", item.Respuesta);
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

		public async Task Actualizar(PreguntaFrecuente item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.PREGUNTA_FRECUENTE SET PREGUNTA = @PREGUNTA, RESPUESTA = @RESPUESTA, HABILITADO = @HABILITADO, ORDEN = @ORDEN, " +
				"FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);

				command.Parameters.AddWithValue("PREGUNTA", item.Pregunta);
				command.Parameters.AddWithValue("RESPUESTA", item.Respuesta);
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
