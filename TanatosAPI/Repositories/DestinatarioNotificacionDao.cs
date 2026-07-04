using Npgsql;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Repositories {
    [ExcludeFromCodeCoverage]
    public class DestinatarioNotificacionDao(IDatabaseConnectionHelper connectionHelper) : IDestinatarioNotificacionDao  {
		public async Task<List<DestinatarioNotificacion>> ObtenerPorSub(string sub, long? idNegocio = null, bool? vigencia = true, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, SUB, ID_NEGOCIO, ID_EMPLEADO, ID_TIPO_RECEPTOR, ALIAS, DESTINO, CODIGO_VALIDACION, FECHA_CADUCIDAD_CODIGO_VALIDACION, " +
				"FECHA_VALIDACION, VALIDADO, HERMES_ID_MENSAJE, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.DESTINATARIO_NOTIFICACION " +
				"WHERE SUB = @SUB AND (ID_NEGOCIO = @IDNEGOCIO OR @IDNEGOCIO IS NULL) AND (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("SUB", sub);
				command.Parameters.AddWithValue("IDNEGOCIO", (object?)idNegocio ?? DBNull.Value);
				command.Parameters.AddWithValue("VIGENCIA", (object?)vigencia ?? DBNull.Value);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				List<DestinatarioNotificacion> retorno = [];
				while (await reader.ReadAsync()) {
					retorno.Add(new DestinatarioNotificacion {
						Id = reader.GetInt64(0),
						Sub = reader.GetString(1),
						IdNegocio = reader.GetInt64(2),
						IdEmpleado = await reader.IsDBNullAsync(3) ? null : reader.GetInt64(3),
						IdTipoReceptor = reader.GetInt64(4),
						Alias = await reader.IsDBNullAsync(5) ? null : reader.GetString(5),
						Destino = reader.GetString(6),
						CodigoValidacion = reader.GetString(7),
						FechaCaducidadCodigoValidacion = reader.GetDateTime(8),
						FechaValidacion = await reader.IsDBNullAsync(9) ? null : reader.GetDateTime(9),
						Validado = reader.GetBoolean(10),
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

		public async Task<DestinatarioNotificacion?> ObtenerPorCodigoValidacion(string codigoValidacion, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, SUB, ID_NEGOCIO, ID_EMPLEADO, ID_TIPO_RECEPTOR, ALIAS, DESTINO, CODIGO_VALIDACION, FECHA_CADUCIDAD_CODIGO_VALIDACION, " +
				"FECHA_VALIDACION, VALIDADO, HERMES_ID_MENSAJE, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.DESTINATARIO_NOTIFICACION " +
				"WHERE CODIGO_VALIDACION = @CODIGOVALIDACION";

			bool disposeConnection = transaction?.Connection == null;
			NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

			try {
				await using NpgsqlCommand command = new(query, connection, transaction);
				command.Parameters.AddWithValue("CODIGOVALIDACION", codigoValidacion);

				await using DbDataReader reader = await command.ExecuteReaderAsync();

				DestinatarioNotificacion? retorno = null;
				if (await reader.ReadAsync()) {
					retorno = new DestinatarioNotificacion {
						Id = reader.GetInt64(0),
						Sub = reader.GetString(1),
						IdNegocio = reader.GetInt64(2),
						IdEmpleado = await reader.IsDBNullAsync(3) ? null : reader.GetInt64(3),
						IdTipoReceptor = reader.GetInt64(4),
						Alias = await reader.IsDBNullAsync(5) ? null : reader.GetString(5),
						Destino = reader.GetString(6),
						CodigoValidacion = reader.GetString(7),
						FechaCaducidadCodigoValidacion = reader.GetDateTime(8),
						FechaValidacion = await reader.IsDBNullAsync(9) ? null : reader.GetDateTime(9),
						Validado = reader.GetBoolean(10),
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

		public async Task<long> Insertar(DestinatarioNotificacion item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.DESTINATARIO_NOTIFICACION(SUB, ID_NEGOCIO, ID_EMPLEADO, ID_TIPO_RECEPTOR, ALIAS, DESTINO, CODIGO_VALIDACION, FECHA_CADUCIDAD_CODIGO_VALIDACION, FECHA_VALIDACION, VALIDADO, HERMES_ID_MENSAJE, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@SUB, @IDNEGOCIO, @IDEMPLEADO, @IDTIPORECEPTOR, @ALIAS, @DESTINO, @CODIGOVALIDACION, @FECHACADUCIDADCODIGOVALIDACION, @FECHAVALIDACION, @VALIDADO, @HERMESIDMENSAJE, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);

                command.Parameters.AddWithValue("SUB", item.Sub);
                command.Parameters.AddWithValue("IDNEGOCIO", item.IdNegocio);
                command.Parameters.AddWithValue("IDEMPLEADO", (object?)item.IdEmpleado ?? DBNull.Value);
                command.Parameters.AddWithValue("IDTIPORECEPTOR", item.IdTipoReceptor);
                command.Parameters.AddWithValue("ALIAS", (object?)item.Alias ?? DBNull.Value);
                command.Parameters.AddWithValue("DESTINO", item.Destino);
                command.Parameters.AddWithValue("CODIGOVALIDACION", item.CodigoValidacion);
                command.Parameters.AddWithValue("FECHACADUCIDADCODIGOVALIDACION", item.FechaCaducidadCodigoValidacion);
                command.Parameters.AddWithValue("FECHAVALIDACION", (object?)item.FechaValidacion ?? DBNull.Value);
                command.Parameters.AddWithValue("VALIDADO", item.Validado);
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

		public async Task Actualizar(DestinatarioNotificacion item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.DESTINATARIO_NOTIFICACION SET SUB = @SUB, ID_NEGOCIO = @IDNEGOCIO, ID_EMPLEADO = @IDEMPLEADO, ID_TIPO_RECEPTOR = @IDTIPORECEPTOR, ALIAS = @ALIAS, DESTINO = @DESTINO, " +
				"CODIGO_VALIDACION = @CODIGOVALIDACION, FECHA_CADUCIDAD_CODIGO_VALIDACION = @FECHACADUCIDADCODIGOVALIDACION, FECHA_VALIDACION = @FECHAVALIDACION, " +
				"VALIDADO = @VALIDADO, HERMES_ID_MENSAJE = @HERMESIDMENSAJE, FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";

            bool disposeConnection = transaction?.Connection == null;
            NpgsqlConnection connection = transaction?.Connection ?? await connectionHelper.ObtenerConexion();

            try {
                await using NpgsqlCommand command = new(query, connection, transaction);

                command.Parameters.AddWithValue("SUB", item.Sub);
                command.Parameters.AddWithValue("IDNEGOCIO", item.IdNegocio);
                command.Parameters.AddWithValue("IDEMPLEADO", (object?)item.IdEmpleado ?? DBNull.Value);
                command.Parameters.AddWithValue("IDTIPORECEPTOR", item.IdTipoReceptor);
                command.Parameters.AddWithValue("ALIAS", (object?)item.Alias ?? DBNull.Value);
                command.Parameters.AddWithValue("DESTINO", item.Destino);
                command.Parameters.AddWithValue("CODIGOVALIDACION", item.CodigoValidacion);
                command.Parameters.AddWithValue("FECHACADUCIDADCODIGOVALIDACION", item.FechaCaducidadCodigoValidacion);
                command.Parameters.AddWithValue("FECHAVALIDACION", (object?)item.FechaValidacion ?? DBNull.Value);
                command.Parameters.AddWithValue("VALIDADO", item.Validado);
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
