using Npgsql;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
    [ExcludeFromCodeCoverage]
    public class HistorialNotificacionDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<List<HistorialNotificacion>> ObtenerPorHistorial(long idHistorialNormaSuscrita, DateTime? fechaEjecucion = null, bool? vigencia = true, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, ID_HISTORIAL_NORMA_SUSCRITA, ID_DESTINATARIO_NOTIFICACION, ID_TIPO_UNIDAD_TIEMPO_ANTELACION, CANT_ANTELACION, " +
                "FECHA_PROGRAMACION, FECHA_EJECUCION, ESTADO, OBSERVACION, CODIGO_ACCESO, FECHA_CADUCIDAD_CODIGO_ACCESO, HERMES_ID_MENSAJE, " +
				"FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.HISTORIAL_NOTIFICACION " +
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
						IdTipoUnidadTiempoAntelacion = await reader.IsDBNullAsync(3) ? null : reader.GetInt64(3),
						CantAntelacion = await reader.IsDBNullAsync(4) ? null : reader.GetInt32(4),
						FechaProgramacion = reader.GetDateTime(5),
						FechaEjecucion = await reader.IsDBNullAsync(6) ? null : reader.GetDateTime(6),
						Estado = await reader.IsDBNullAsync(7) ? null : reader.GetInt16(7),
						Observacion = await reader.IsDBNullAsync(8) ? null : reader.GetString(8),
						CodigoAcceso = await reader.IsDBNullAsync(9) ? null : reader.GetString(9),
						FechaCaducidadCodigoAcceso = await reader.IsDBNullAsync(10) ? null : reader.GetDateTime(10),
                        HermesIdMensaje = await reader.IsDBNullAsync(11) ? null : reader.GetString(11),
						FechaCreacion = reader.GetDateTime(12),
						FechaEliminacion = await reader.IsDBNullAsync(13) ? null : reader.GetDateTime(13),
						Vigencia = reader.GetBoolean(14)
					});
				}
				return retorno;
			} finally {
				if (disposeConnection && connection != null) {
					await connection.DisposeAsync();
				}
			}
		}

        public async Task<HistorialNotificacion?> ObtenerPorCodigoAcceso(string codigoAcceso, bool? vigencia = true, NpgsqlTransaction? transaction = null) {
            string query =
                "SELECT ID, ID_HISTORIAL_NORMA_SUSCRITA, ID_DESTINATARIO_NOTIFICACION, ID_TIPO_UNIDAD_TIEMPO_ANTELACION, CANT_ANTELACION, " +
                "FECHA_PROGRAMACION, FECHA_EJECUCION, ESTADO, OBSERVACION, CODIGO_ACCESO, FECHA_CADUCIDAD_CODIGO_ACCESO, HERMES_ID_MENSAJE, " +
                "FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.HISTORIAL_NOTIFICACION " +
                "WHERE CODIGO_ACCESO = @CODIGOACCESO AND (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);

                command.Parameters.AddWithValue("CODIGOACCESO", codigoAcceso);
                command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);

                await using DbDataReader reader = await command.ExecuteReaderAsync();

                HistorialNotificacion? retorno = null;
                if (await reader.ReadAsync()) {
                    retorno = new HistorialNotificacion {
                        Id = reader.GetInt64(0),
                        IdHistorialNormaSuscrita = reader.GetInt64(1),
                        IdDestinatarioNotificacion = reader.GetInt64(2),
                        IdTipoUnidadTiempoAntelacion = await reader.IsDBNullAsync(3) ? null : reader.GetInt64(3),
                        CantAntelacion = await reader.IsDBNullAsync(4) ? null : reader.GetInt32(4),
                        FechaProgramacion = reader.GetDateTime(5),
                        FechaEjecucion = await reader.IsDBNullAsync(6) ? null : reader.GetDateTime(6),
                        Estado = await reader.IsDBNullAsync(7) ? null : reader.GetInt16(7),
                        Observacion = await reader.IsDBNullAsync(8) ? null : reader.GetString(8),
                        CodigoAcceso = await reader.IsDBNullAsync(9) ? null : reader.GetString(9),
                        FechaCaducidadCodigoAcceso = await reader.IsDBNullAsync(10) ? null : reader.GetDateTime(10),
                        HermesIdMensaje = await reader.IsDBNullAsync(11) ? null : reader.GetString(11),
                        FechaCreacion = reader.GetDateTime(12),
                        FechaEliminacion = await reader.IsDBNullAsync(13) ? null : reader.GetDateTime(13),
                        Vigencia = reader.GetBoolean(14)
                    };
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
                "INSERT INTO TANATOS.HISTORIAL_NOTIFICACION(ID_HISTORIAL_NORMA_SUSCRITA, ID_DESTINATARIO_NOTIFICACION, ID_TIPO_UNIDAD_TIEMPO_ANTELACION, CANT_ANTELACION, FECHA_PROGRAMACION, FECHA_EJECUCION, ESTADO, OBSERVACION, CODIGO_ACCESO, FECHA_CADUCIDAD_CODIGO_ACCESO, HERMES_ID_MENSAJE, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
                "VALUES (@IDHISTORIALNORMASUSCRITA, @IDDESTINATARIONOTIFICACION, @IDTIPOUNIDADTIEMPOANTELACION, @CANTANTELACION, @FECHAPROGRAMACION, @FECHAEJECUCION, @ESTADO, @OBSERVACION, @CODIGOACCESO, @FECHACADUCIDADCODIGOACCESO, @HERMESIDMENSAJE, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("IDHISTORIALNORMASUSCRITA", item.IdHistorialNormaSuscrita);
                command.Parameters.AddWithValue("IDDESTINATARIONOTIFICACION", item.IdDestinatarioNotificacion);
                command.Parameters.AddWithValue("IDTIPOUNIDADTIEMPOANTELACION", (object?)item.IdTipoUnidadTiempoAntelacion ?? DBNull.Value);
                command.Parameters.AddWithValue("CANTANTELACION", (object?)item.CantAntelacion ?? DBNull.Value);
                command.Parameters.AddWithValue("FECHAPROGRAMACION", item.FechaProgramacion);
                command.Parameters.AddWithValue("FECHAEJECUCION", (object?)item.FechaEjecucion ?? DBNull.Value);
                command.Parameters.AddWithValue("ESTADO", (object?)item.Estado ?? DBNull.Value);
                command.Parameters.AddWithValue("OBSERVACION", (object?)item.Observacion ?? DBNull.Value);
                command.Parameters.AddWithValue("CODIGOACCESO", (object?)item.CodigoAcceso ?? DBNull.Value);
                command.Parameters.AddWithValue("FECHACADUCIDADCODIGOACCESO", (object?)item.FechaCaducidadCodigoAcceso ?? DBNull.Value);
                command.Parameters.AddWithValue("HERMESIDMENSAJE", (object?)item.HermesIdMensaje ?? DBNull.Value);
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

		public async Task Actualizar(HistorialNotificacion item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.HISTORIAL_NOTIFICACION SET ID_HISTORIAL_NORMA_SUSCRITA = @IDHISTORIALNORMASUSCRITA, " +
                "ID_DESTINATARIO_NOTIFICACION = @IDDESTINATARIONOTIFICACION, " +
				"ID_TIPO_UNIDAD_TIEMPO_ANTELACION = @IDTIPOUNIDADTIEMPOANTELACION, CANT_ANTELACION = @CANTANTELACION, " +
                "FECHA_PROGRAMACION = @FECHAPROGRAMACION, FECHA_EJECUCION = @FECHAEJECUCION, " +
                "ESTADO = @ESTADO, OBSERVACION = @OBSERVACION, CODIGO_ACCESO = @CODIGOACCESO, " +
                "FECHA_CADUCIDAD_CODIGO_ACCESO = @FECHACADUCIDADCODIGOACCESO, HERMES_ID_MENSAJE = @HERMESIDMENSAJE, " +
				"FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);
                command.Parameters.AddWithValue("IDHISTORIALNORMASUSCRITA", item.IdHistorialNormaSuscrita);
                command.Parameters.AddWithValue("IDDESTINATARIONOTIFICACION", item.IdDestinatarioNotificacion);
                command.Parameters.AddWithValue("IDTIPOUNIDADTIEMPOANTELACION", (object?)item.IdTipoUnidadTiempoAntelacion ?? DBNull.Value);
                command.Parameters.AddWithValue("CANTANTELACION", (object?)item.CantAntelacion ?? DBNull.Value);
                command.Parameters.AddWithValue("FECHAPROGRAMACION", item.FechaProgramacion);
                command.Parameters.AddWithValue("FECHAEJECUCION", (object?)item.FechaEjecucion ?? DBNull.Value);
                command.Parameters.AddWithValue("ESTADO", (object?)item.Estado ?? DBNull.Value);
                command.Parameters.AddWithValue("OBSERVACION", (object?)item.Observacion ?? DBNull.Value);
                command.Parameters.AddWithValue("CODIGOACCESO", (object?)item.CodigoAcceso ?? DBNull.Value);
                command.Parameters.AddWithValue("FECHACADUCIDADCODIGOACCESO", (object?)item.FechaCaducidadCodigoAcceso ?? DBNull.Value);
                command.Parameters.AddWithValue("HERMESIDMENSAJE", (object?)item.HermesIdMensaje ?? DBNull.Value);
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
