using Dapper;
using Npgsql;
using System.Data;
using System.Data.Common;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class HistorialNotificacionDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<List<HistorialNotificacion>> ObtenerPorHistorial(long idHistorialNormaSuscrita, DateTime? fechaEjecucion = null, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, ID_HISTORIAL_NORMA_SUSCRITA, ID_DESTINATARIO_NOTIFICACION, ID_TIPO_UNIDAD_TIEMPO_ANTELACION, CANT_ANTELACION, FECHA_PROGRAMACION, FECHA_EJECUCION FROM TANATOS.HISTORIAL_NOTIFICACION " +
				"WHERE ID_HISTORIAL_NORMA_SUSCRITA = @IDHISTORIALNORMASUSCRITA AND FECHA_EJECUCION IS NOT DISTINCT FROM @FECHAEJECUCION";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);

				command.Parameters.AddWithValue("IDHISTORIALNORMASUSCRITA", idHistorialNormaSuscrita);
				command.Parameters.AddWithValue("FECHAEJECUCION", (object?)fechaEjecucion ?? DBNull.Value);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				List<HistorialNotificacion> retorno = [];

				while (await reader.ReadAsync()) {
					retorno.Add(new HistorialNotificacion {
						Id = reader.GetInt64(0),
						IdHistorialNormaSuscrita = reader.GetInt64(1),
						IdDestinatarioNotificacion = reader.GetInt64(2),
						IdTipoUnidadTiempoAntelacion = reader.IsDBNull(3) ? null : reader.GetInt64(3),
						CantAntelacion = reader.IsDBNull(4) ? null : reader.GetInt32(4),
						FechaProgramacion = reader.GetDateTime(5),
						FechaEjecucion = reader.IsDBNull(6) ? null : reader.GetDateTime(6),
					});
				}

				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

		public async Task<long> Insertar(HistorialNotificacion item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.HISTORIAL_NOTIFICACION(ID_HISTORIAL_NORMA_SUSCRITA, ID_DESTINATARIO_NOTIFICACION, ID_TIPO_UNIDAD_TIEMPO_ANTELACION, CANT_ANTELACION, FECHA_PROGRAMACION, FECHA_EJECUCION) " +
				"VALUES (@IDHISTORIALNORMASUSCRITA, @IDDESTINATARIONOTIFICACION, @IDTIPOUNIDADTIEMPOANTELACION, @CANTANTELACION, @FECHAPROGRAMACION, @FECHAEJECUCION) " +
				"RETURNING ID";
			DynamicParameters param = new();
			param.Add("IDHISTORIALNORMASUSCRITA", item.IdHistorialNormaSuscrita);
			param.Add("IDDESTINATARIONOTIFICACION", item.IdDestinatarioNotificacion);
			param.Add("IDTIPOUNIDADTIEMPOANTELACION", item.IdTipoUnidadTiempoAntelacion);
			param.Add("CANTANTELACION", item.CantAntelacion);
			param.Add("FECHAPROGRAMACION", item.FechaProgramacion);
			param.Add("FECHAEJECUCION", item.FechaEjecucion);

			if (transaction?.Connection != null) {
				return await transaction!.Connection!.ExecuteScalarAsync<long>(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				return await connection.ExecuteScalarAsync<long>(query, param);
			}
		}

		public async Task Actualizar(HistorialNotificacion item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.HISTORIAL_NOTIFICACION SET ID_HISTORIAL_NORMA_SUSCRITA = @IDHISTORIALNORMASUSCRITA, ID_DESTINATARIO_NOTIFICACION = @IDDESTINATARIONOTIFICACION, " +
				"ID_TIPO_UNIDAD_TIEMPO_ANTELACION = @IDTIPOUNIDADTIEMPOANTELACION, CANT_ANTELACION = @CANTANTELACION, FECHA_PROGRAMACION = @FECHAPROGRAMACION, FECHA_EJECUCION = @FECHAEJECUCION " +
				"WHERE ID = @ID";
			DynamicParameters param = new();
			param.Add("IDHISTORIALNORMASUSCRITA", item.IdHistorialNormaSuscrita);
			param.Add("IDDESTINATARIONOTIFICACION", item.IdDestinatarioNotificacion);
			param.Add("IDTIPOUNIDADTIEMPOANTELACION", item.IdTipoUnidadTiempoAntelacion);
			param.Add("CANTANTELACION", item.CantAntelacion);
			param.Add("FECHAPROGRAMACION", item.FechaProgramacion);
			param.Add("FECHAEJECUCION", item.FechaEjecucion);
			param.Add("ID", item.Id);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}

		public async Task Eliminar(long id, NpgsqlTransaction? transaction = null) {
			string query = "DELETE FROM TANATOS.HISTORIAL_NOTIFICACION WHERE ID = @ID";
			DynamicParameters param = new();
			param.Add("ID", id);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}
	}
}
