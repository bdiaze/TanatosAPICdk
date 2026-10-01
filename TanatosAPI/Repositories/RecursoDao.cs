using Npgsql;
using System.Data.Common;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Repositories {
	public class RecursoDao(IDatabaseConnectionHelper connectionHelper) : IRecursoDao {
		public async Task<List<Recurso>> ObtenerVarios(HashSet<long> ids, NpgsqlTransaction? transaction = null) {
			if (ids.Count == 0) return [];

			string query =
				"SELECT ID, TIPO, ID_INTERNO, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA " +
				"FROM TANATOS.RECURSO " +
				"WHERE ID = ANY(@IDS)";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("IDS", ids.ToArray());

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				List<Recurso> retorno = [];
				while (await reader.ReadAsync()) {
					retorno.Add(new Recurso {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						Tipo = reader.GetString(reader.GetOrdinal("TIPO")),
						IdInterno = reader.GetString(reader.GetOrdinal("ID_INTERNO")),
						FechaCreacion = reader.GetDateTime(reader.GetOrdinal("FECHA_CREACION")),
						FechaEliminacion = await reader.IsDBNullAsync(reader.GetOrdinal("FECHA_ELIMINACION")) ? null : reader.GetDateTime(reader.GetOrdinal("FECHA_ELIMINACION")),
						Vigencia = reader.GetBoolean(reader.GetOrdinal("VIGENCIA"))
					});
				}
				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<long> Insertar(Recurso item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.RECURSO(TIPO, ID_INTERNO, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@TIPO, @IDINTERNO, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("TIPO", item.Tipo);
				command.Parameters.AddWithValue("IDINTERNO", item.IdInterno);
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

		public async Task Actualizar(Recurso item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.RECURSO SET TIPO = @TIPO, ID_INTERNO = @IDINTERNO, " +
				"FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("TIPO", item.Tipo);
				command.Parameters.AddWithValue("IDINTERNO", item.IdInterno);
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
