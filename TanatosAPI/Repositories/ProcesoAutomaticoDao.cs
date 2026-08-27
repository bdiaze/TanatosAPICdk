using Npgsql;
using System.Data.Common;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Repositories {
	public class ProcesoAutomaticoDao(IDatabaseConnectionHelper connectionHelper) : IProcesoAutomaticoDao {
		public async Task<List<ProcesoAutomatico>> ObtenerVarios(HashSet<long> ids, NpgsqlTransaction? transaction = null) {
			if (ids.Count == 0) return [];

			string query =
				"SELECT ID, ID_TIPO_PROCESO_AUTOMATICO, ID_PROCESO_KAIROS, ID_CALENDARIZACION_KAIROS, NOMBRE, ARN_ROL, ARN_PROCESO, PARAMETROS, " +
				"CRON, FRECUENCIA_DIAS, INICIO_EJECUCION_UTC, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA " +
				"FROM TANATOS.PROCESO_AUTOMATICO WHERE ID = ANY(@IDS)";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("IDS", ids.ToArray());

				await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

				List<ProcesoAutomatico> retorno = [];
				while (await reader.ReadAsync()) {
					retorno.Add(new ProcesoAutomatico {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						IdTipoProcesoAutomatico = reader.GetInt64(reader.GetOrdinal("ID_TIPO_PROCESO_AUTOMATICO")),
						IdProcesoKairos = reader.GetString(reader.GetOrdinal("ID_PROCESO_KAIROS")),
						IdCalendarizacionKairos = reader.GetString(reader.GetOrdinal("ID_CALENDARIZACION_KAIROS")),
						Nombre = reader.GetString(reader.GetOrdinal("NOMBRE")),
						ArnRol = reader.GetString(reader.GetOrdinal("ARN_ROL")),
						ArnProceso = reader.GetString(reader.GetOrdinal("ARN_PROCESO")),
						Parametros = reader.GetString(reader.GetOrdinal("PARAMETROS")),
						Cron = await reader.IsDBNullAsync(reader.GetOrdinal("CRON")) ? null : reader.GetString(reader.GetOrdinal("CRON")),
						FrecuenciaDias = await reader.IsDBNullAsync(reader.GetOrdinal("FRECUENCIA_DIAS")) ? null : reader.GetInt32(reader.GetOrdinal("FRECUENCIA_DIAS")),
						InicioEjecucionUtc = await reader.IsDBNullAsync(reader.GetOrdinal("INICIO_EJECUCION_UTC")) ? null : reader.GetDateTime(reader.GetOrdinal("INICIO_EJECUCION_UTC")),
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

		public async Task<List<ProcesoAutomatico>> ObtenerPorNombre(string nombre, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, ID_TIPO_PROCESO_AUTOMATICO, ID_PROCESO_KAIROS, ID_CALENDARIZACION_KAIROS, NOMBRE, ARN_ROL, ARN_PROCESO, PARAMETROS, " +
				"CRON, FRECUENCIA_DIAS, INICIO_EJECUCION_UTC, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA " +
				"FROM TANATOS.PROCESO_AUTOMATICO WHERE NOMBRE = @NOMBRE";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("NOMBRE", nombre);

				await using NpgsqlDataReader reader = await command.ExecuteReaderAsync();

				List<ProcesoAutomatico> retorno = [];
				while (await reader.ReadAsync()) {
					retorno.Add(new ProcesoAutomatico {
						Id = reader.GetInt64(reader.GetOrdinal("ID")),
						IdTipoProcesoAutomatico = reader.GetInt64(reader.GetOrdinal("ID_TIPO_PROCESO_AUTOMATICO")),
						IdProcesoKairos = reader.GetString(reader.GetOrdinal("ID_PROCESO_KAIROS")),
						IdCalendarizacionKairos = reader.GetString(reader.GetOrdinal("ID_CALENDARIZACION_KAIROS")),
						Nombre = reader.GetString(reader.GetOrdinal("NOMBRE")),
						ArnRol = reader.GetString(reader.GetOrdinal("ARN_ROL")),
						ArnProceso = reader.GetString(reader.GetOrdinal("ARN_PROCESO")),
						Parametros = reader.GetString(reader.GetOrdinal("PARAMETROS")),
						Cron = await reader.IsDBNullAsync(reader.GetOrdinal("CRON")) ? null : reader.GetString(reader.GetOrdinal("CRON")),
						FrecuenciaDias = await reader.IsDBNullAsync(reader.GetOrdinal("FRECUENCIA_DIAS")) ? null : reader.GetInt32(reader.GetOrdinal("FRECUENCIA_DIAS")),
						InicioEjecucionUtc = await reader.IsDBNullAsync(reader.GetOrdinal("INICIO_EJECUCION_UTC")) ? null : reader.GetDateTime(reader.GetOrdinal("INICIO_EJECUCION_UTC")),
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

		public async Task<long> Insertar(ProcesoAutomatico item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.PROCESO_AUTOMATICO(ID_TIPO_PROCESO_AUTOMATICO, ID_PROCESO_KAIROS, ID_CALENDARIZACION_KAIROS, NOMBRE, ARN_ROL, ARN_PROCESO, PARAMETROS, CRON, FRECUENCIA_DIAS, INICIO_EJECUCION_UTC, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@IDTIPOPROCESOAUTOMATICO, @IDPROCESOKAIROS, @IDCALENDARIZACIONKAIROS, @NOMBRE, @ARNROL, @ARNPROCESO, @PARAMETROS, @CRON, @FRECUENCIADIAS, @INICIOEJECUCIONUTC, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);

				command.Parameters.AddWithValue("IDTIPOPROCESOAUTOMATICO", item.IdTipoProcesoAutomatico);
				command.Parameters.AddWithValue("IDPROCESOKAIROS", item.IdProcesoKairos);
				command.Parameters.AddWithValue("IDCALENDARIZACIONKAIROS", item.IdCalendarizacionKairos);
				command.Parameters.AddWithValue("NOMBRE", item.Nombre);
				command.Parameters.AddWithValue("ARNROL", item.ArnRol);
				command.Parameters.AddWithValue("ARNPROCESO", item.ArnProceso);
				command.Parameters.AddWithValue("PARAMETROS", item.Parametros);
				command.Parameters.AddWithValue("CRON", (object?)item.Cron ?? DBNull.Value);
				command.Parameters.AddWithValue("FRECUENCIADIAS", (object?)item.FrecuenciaDias ?? DBNull.Value);
				command.Parameters.AddWithValue("INICIOEJECUCIONUTC", (object?)item.InicioEjecucionUtc ?? DBNull.Value);
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

		public async Task Actualizar(ProcesoAutomatico item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.PROCESO_AUTOMATICO SET ID_TIPO_PROCESO_AUTOMATICO = @IDTIPOPROCESOAUTOMATICO, ID_PROCESO_KAIROS = @IDPROCESOKAIROS, ID_CALENDARIZACION_KAIROS = @IDCALENDARIZACIONKAIROS, " +
				"NOMBRE = @NOMBRE, ARN_ROL = @ARNROL, ARN_PROCESO = @ARNPROCESO, PARAMETROS = @PARAMETROS, CRON = @CRON, FRECUENCIA_DIAS = @FRECUENCIADIAS, INICIO_EJECUCION_UTC = @INICIOEJECUCIONUTC, " +
				"FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);

				command.Parameters.AddWithValue("IDTIPOPROCESOAUTOMATICO", item.IdTipoProcesoAutomatico);
				command.Parameters.AddWithValue("IDPROCESOKAIROS", item.IdProcesoKairos);
				command.Parameters.AddWithValue("IDCALENDARIZACIONKAIROS", item.IdCalendarizacionKairos);
				command.Parameters.AddWithValue("NOMBRE", item.Nombre);
				command.Parameters.AddWithValue("ARNROL", item.ArnRol);
				command.Parameters.AddWithValue("ARNPROCESO", item.ArnProceso);
				command.Parameters.AddWithValue("PARAMETROS", item.Parametros);
				command.Parameters.AddWithValue("CRON", (object?)item.Cron ?? DBNull.Value);
				command.Parameters.AddWithValue("FRECUENCIADIAS", (object?)item.FrecuenciaDias ?? DBNull.Value);
				command.Parameters.AddWithValue("INICIOEJECUCIONUTC", (object?)item.InicioEjecucionUtc ?? DBNull.Value);
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
