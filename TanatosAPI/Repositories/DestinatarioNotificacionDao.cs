using Dapper;
using Npgsql;
using System.Data.Common;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class DestinatarioNotificacionDao(DatabaseConnectionHelper connectionHelper) {
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
			DynamicParameters param = new();
			param.Add("SUB", item.Sub);
			param.Add("IDNEGOCIO", item.IdNegocio);
			param.Add("IDEMPLEADO", item.IdEmpleado);
			param.Add("IDTIPORECEPTOR", item.IdTipoReceptor);
			param.Add("ALIAS", item.Alias);
			param.Add("DESTINO", item.Destino);
			param.Add("CODIGOVALIDACION", item.CodigoValidacion);
			param.Add("FECHACADUCIDADCODIGOVALIDACION", item.FechaCaducidadCodigoValidacion);
			param.Add("FECHAVALIDACION", item.FechaValidacion);
			param.Add("VALIDADO", item.Validado);
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

		public async Task Actualizar(DestinatarioNotificacion item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.DESTINATARIO_NOTIFICACION SET SUB = @SUB, ID_NEGOCIO = @IDNEGOCIO, ID_EMPLEADO = @IDEMPLEADO, ID_TIPO_RECEPTOR = @IDTIPORECEPTOR, ALIAS = @ALIAS, DESTINO = @DESTINO, " +
				"CODIGO_VALIDACION = @CODIGOVALIDACION, FECHA_CADUCIDAD_CODIGO_VALIDACION = @FECHACADUCIDADCODIGOVALIDACION, FECHA_VALIDACION = @FECHAVALIDACION, " +
				"VALIDADO = @VALIDADO, HERMES_ID_MENSAJE = @HERMESIDMENSAJE, FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";
			DynamicParameters param = new();
			param.Add("SUB", item.Sub);
			param.Add("IDNEGOCIO", item.IdNegocio);
			param.Add("IDEMPLEADO", item.IdEmpleado);
			param.Add("IDTIPORECEPTOR", item.IdTipoReceptor);
			param.Add("ALIAS", item.Alias);
			param.Add("DESTINO", item.Destino);
			param.Add("CODIGOVALIDACION", item.CodigoValidacion);
			param.Add("FECHACADUCIDADCODIGOVALIDACION", item.FechaCaducidadCodigoValidacion);
			param.Add("FECHAVALIDACION", item.FechaValidacion);
			param.Add("VALIDADO", item.Validado);
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
