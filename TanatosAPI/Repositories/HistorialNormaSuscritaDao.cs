using Dapper;
using Npgsql;
using System.Data.Common;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class HistorialNormaSuscritaDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<HistorialNormaSuscrita?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, ID_NORMA_SUSCRITA, FECHA_VENCIMIENTO, FECHA_COMPLETITUD, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.HISTORIAL_NORMA_SUSCRITA " +
				"WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);

				command.Parameters.AddWithValue("ID", id);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				HistorialNormaSuscrita? retorno = null;

				if (await reader.ReadAsync()) {
					retorno = new HistorialNormaSuscrita {
						Id = reader.GetInt64(0),
						IdNormaSuscrita = reader.GetInt64(1),
						FechaVencimiento = reader.GetDateTime(2),
						FechaCompletitud = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
						FechaCreacion = reader.GetDateTime(4),
						FechaEliminacion = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
						Vigencia = reader.GetBoolean(6)
					};
				}

				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<List<HistorialNormaSuscrita>> ObtenerPorNormaSuscrita(long idNormaSuscrita, DateTime? fechaVencimiento = null, bool vigencia = true) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<HistorialNormaSuscrita>(
				"SELECT ID, ID_NORMA_SUSCRITA, FECHA_VENCIMIENTO, FECHA_COMPLETITUD, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.HISTORIAL_NORMA_SUSCRITA " +
				"WHERE ID_NORMA_SUSCRITA = @IDNORMASUSCRITA AND (FECHA_VENCIMIENTO = @FECHAVENCIMIENTO OR @FECHAVENCIMIENTO IS NULL) AND (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)",
				new { idNormaSuscrita, fechaVencimiento, vigencia }
			)];
		}

		public async Task<List<HistorialNormaSuscrita>> ObtenerPorNormaSuscritaYFechaCompletitud(long idNormaSuscrita, DateTime? fechaCompletitud, bool vigencia = true, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, ID_NORMA_SUSCRITA, FECHA_VENCIMIENTO, FECHA_COMPLETITUD, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.HISTORIAL_NORMA_SUSCRITA " +
				"WHERE ID_NORMA_SUSCRITA = @IDNORMASUSCRITA AND FECHA_COMPLETITUD IS NOT DISTINCT FROM @FECHACOMPLETITUD  AND (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);

				command.Parameters.AddWithValue("IDNORMASUSCRITA", idNormaSuscrita);
				command.Parameters.AddWithValue("FECHACOMPLETITUD", (object?)fechaCompletitud ?? DBNull.Value);
				command.Parameters.AddWithValue("VIGENCIA", vigencia);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				List<HistorialNormaSuscrita> retorno = [];

				while (await reader.ReadAsync()) {
					retorno.Add(new HistorialNormaSuscrita {
						Id = reader.GetInt64(0),
						IdNormaSuscrita = reader.GetInt64(1),
						FechaVencimiento = reader.GetDateTime(2),
						FechaCompletitud = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
						FechaCreacion = reader.GetDateTime(4),
						FechaEliminacion = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
						Vigencia = reader.GetBoolean(6)
					});
				}

				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<long> Insertar(HistorialNormaSuscrita item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.HISTORIAL_NORMA_SUSCRITA(ID_NORMA_SUSCRITA, FECHA_VENCIMIENTO, FECHA_COMPLETITUD, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@IDNORMASUSCRITA, @FECHAVENCIMIENTO, @FECHACOMPLETITUD, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";
			DynamicParameters param = new();
			param.Add("IDNORMASUSCRITA", item.IdNormaSuscrita);
			param.Add("FECHAVENCIMIENTO", item.FechaVencimiento);
			param.Add("FECHACOMPLETITUD", item.FechaCompletitud);
			param.Add("FECHACREACION", item.FechaCreacion);
			param.Add("FECHAELIMINACION", item.FechaEliminacion);
			param.Add("VIGENCIA", item.Vigencia);

			if (transaction?.Connection != null) {
				return await transaction!.Connection!.ExecuteScalarAsync<long>(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				return await connection.ExecuteScalarAsync<long>(query, param);
			}
		}

		public async Task Actualizar(HistorialNormaSuscrita item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.HISTORIAL_NORMA_SUSCRITA SET ID_NORMA_SUSCRITA = @IDNORMASUSCRITA, FECHA_VENCIMIENTO = @FECHAVENCIMIENTO, FECHA_COMPLETITUD = @FECHACOMPLETITUD, " +
				"FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";
			DynamicParameters param = new();
			param.Add("IDNORMASUSCRITA", item.IdNormaSuscrita);
			param.Add("FECHAVENCIMIENTO", item.FechaVencimiento);
			param.Add("FECHACOMPLETITUD", item.FechaCompletitud);
			param.Add("FECHACREACION", item.FechaCreacion);
			param.Add("FECHAELIMINACION", item.FechaEliminacion);
			param.Add("VIGENCIA", item.Vigencia);
			param.Add("ID", item.Id);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}
	}
}
