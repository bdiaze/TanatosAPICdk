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
		public async Task<List<HistorialNotificacion>> ObtenerPorHistorial(long idHistorialNormaSuscrita, DateTime? fechaEjecucion = null, bool? vigencia = true, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, ID_HISTORIAL_NORMA_SUSCRITA, ID_DESTINATARIO_NOTIFICACION, ID_TIPO_UNIDAD_TIEMPO_ANTELACION, CANT_ANTELACION, " +
				"FECHA_PROGRAMACION, FECHA_EJECUCION, ESTADO, HERMES_ID_MENSAJE, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.HISTORIAL_NOTIFICACION " +
				"WHERE ID_HISTORIAL_NORMA_SUSCRITA = @IDHISTORIALNORMASUSCRITA AND FECHA_EJECUCION IS NOT DISTINCT FROM @FECHAEJECUCION AND (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);

				command.Parameters.AddWithValue("IDHISTORIALNORMASUSCRITA", idHistorialNormaSuscrita);
				command.Parameters.AddWithValue("FECHAEJECUCION", (object?)fechaEjecucion ?? DBNull.Value);
				command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);

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
						Estado = reader.IsDBNull(7) ? null : reader.GetInt16(7),
						HermesIdMensaje = reader.IsDBNull(8) ? null : reader.GetString(8),
						FechaCreacion = reader.GetDateTime(9),
						FechaEliminacion = reader.IsDBNull(10) ? null : reader.GetDateTime(10),
						Vigencia = reader.GetBoolean(11)
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
				"INSERT INTO TANATOS.HISTORIAL_NOTIFICACION(ID_HISTORIAL_NORMA_SUSCRITA, ID_DESTINATARIO_NOTIFICACION, ID_TIPO_UNIDAD_TIEMPO_ANTELACION, CANT_ANTELACION, FECHA_PROGRAMACION, FECHA_EJECUCION, ESTADO, HERMES_ID_MENSAJE, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@IDHISTORIALNORMASUSCRITA, @IDDESTINATARIONOTIFICACION, @IDTIPOUNIDADTIEMPOANTELACION, @CANTANTELACION, @FECHAPROGRAMACION, @FECHAEJECUCION, @ESTADO, @HERMESIDMENSAJE, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";
			DynamicParameters param = new();
			param.Add("IDHISTORIALNORMASUSCRITA", item.IdHistorialNormaSuscrita);
			param.Add("IDDESTINATARIONOTIFICACION", item.IdDestinatarioNotificacion);
			param.Add("IDTIPOUNIDADTIEMPOANTELACION", item.IdTipoUnidadTiempoAntelacion);
			param.Add("CANTANTELACION", item.CantAntelacion);
			param.Add("FECHAPROGRAMACION", item.FechaProgramacion);
			param.Add("FECHAEJECUCION", item.FechaEjecucion);
			param.Add("ESTADO", item.Estado);
			param.Add("HERMESIDMENSAJE", item.HermesIdMensaje);
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

		public async Task Actualizar(HistorialNotificacion item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.HISTORIAL_NOTIFICACION SET ID_HISTORIAL_NORMA_SUSCRITA = @IDHISTORIALNORMASUSCRITA, ID_DESTINATARIO_NOTIFICACION = @IDDESTINATARIONOTIFICACION, " +
				"ID_TIPO_UNIDAD_TIEMPO_ANTELACION = @IDTIPOUNIDADTIEMPOANTELACION, CANT_ANTELACION = @CANTANTELACION, FECHA_PROGRAMACION = @FECHAPROGRAMACION, FECHA_EJECUCION = @FECHAEJECUCION, " +
				"ESTADO = @ESTADO, HERMES_ID_MENSAJE = @HERMESIDMENSAJE, FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";
			DynamicParameters param = new();
			param.Add("IDHISTORIALNORMASUSCRITA", item.IdHistorialNormaSuscrita);
			param.Add("IDDESTINATARIONOTIFICACION", item.IdDestinatarioNotificacion);
			param.Add("IDTIPOUNIDADTIEMPOANTELACION", item.IdTipoUnidadTiempoAntelacion);
			param.Add("CANTANTELACION", item.CantAntelacion);
			param.Add("FECHAPROGRAMACION", item.FechaProgramacion);
			param.Add("FECHAEJECUCION", item.FechaEjecucion);
			param.Add("ESTADO", item.Estado);
			param.Add("HERMESIDMENSAJE", item.HermesIdMensaje);
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
