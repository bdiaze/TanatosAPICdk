using Npgsql;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TanatosAPI.Repositories {
	[ExcludeFromCodeCoverage]
	public class NormaSuscritaProcesoNotificacionDao(IDatabaseConnectionHelper connectionHelper) : INormaSuscritaProcesoNotificacionDao {
		public async Task<List<NormaSuscritaProcesoNotificacion>> ObtenerPorNormaSuscrita(long idNormaSuscrita, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, ID_NORMA_SUSCRITA, ID_PROCESO_AUTOMATICO, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA " +
				"FROM TANATOS.NORMA_SUSCRITA_PROCESO_NOTIFICACION WHERE ID_NORMA_SUSCRITA = @IDNORMASUSCRITA";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("IDNORMASUSCRITA", idNormaSuscrita);

				await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

				List<NormaSuscritaProcesoNotificacion> retorno = [];
				while (await reader.ReadAsync()) {
					retorno.Add(new NormaSuscritaProcesoNotificacion {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						IdNormaSuscrita = reader.GetInt64(reader.GetOrdinal("ID_NORMA_SUSCRITA")),
						IdProcesoAutomatico = reader.GetInt64(reader.GetOrdinal("ID_PROCESO_AUTOMATICO")),
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

		public async Task<long> Insertar(NormaSuscritaProcesoNotificacion item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.NORMA_SUSCRITA_PROCESO_NOTIFICACION(ID_NORMA_SUSCRITA, ID_PROCESO_AUTOMATICO, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@IDNORMASUSCRITA, @IDPROCESOAUTOMATICO, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);

				command.Parameters.AddWithValue("IDNORMASUSCRITA", item.IdNormaSuscrita);
				command.Parameters.AddWithValue("IDPROCESOAUTOMATICO", item.IdProcesoAutomatico);
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

		public async Task Actualizar(NormaSuscritaProcesoNotificacion item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.NORMA_SUSCRITA_PROCESO_NOTIFICACION SET ID_NORMA_SUSCRITA = @IDNORMASUSCRITA, ID_PROCESO_AUTOMATICO = @IDPROCESOAUTOMATICO, " +
				"FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);

				command.Parameters.AddWithValue("IDNORMASUSCRITA", item.IdNormaSuscrita);
				command.Parameters.AddWithValue("IDPROCESOAUTOMATICO", item.IdProcesoAutomatico);
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
