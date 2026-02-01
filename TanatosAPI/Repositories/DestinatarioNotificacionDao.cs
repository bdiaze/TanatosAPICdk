using Dapper;
using Npgsql;
using System.Data.Common;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class DestinatarioNotificacionDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<List<DestinatarioNotificacion>> ObtenerPorSub(string sub, long? idNegocio = null, bool? vigencia = true, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, SUB, ID_NEGOCIO, ID_TIPO_RECEPTOR, DESTINO, CODIGO_VALIDACION, FECHA_CADUCIDAD_CODIGO_VALIDACION, FECHA_VALIDACION, VALIDADO, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.DESTINATARIO_NOTIFICACION " +
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
						IdTipoReceptor = reader.GetInt64(3),
						Destino = reader.GetString(4),
						CodigoValidacion = reader.GetString(5),
						FechaCaducidadCodigoValidacion = reader.GetDateTime(6),
						FechaValidacion = reader.IsDBNull(7) ? null : reader.GetDateTime(7),
						Validado = reader.GetBoolean(8),
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

		public async Task<DestinatarioNotificacion?> ObtenerPorCodigoValidacion(string codigoValidacion) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return await connection.QueryFirstOrDefaultAsync<DestinatarioNotificacion>(
				"SELECT ID, SUB, ID_NEGOCIO, ID_TIPO_RECEPTOR, DESTINO, CODIGO_VALIDACION, FECHA_CADUCIDAD_CODIGO_VALIDACION, FECHA_VALIDACION, VALIDADO, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.DESTINATARIO_NOTIFICACION WHERE CODIGO_VALIDACION = @CODIGOVALIDACION",
				new { codigoValidacion }
			);
		}

		public async Task<long> Insertar(DestinatarioNotificacion item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return await connection.ExecuteScalarAsync<long>(
				"INSERT INTO TANATOS.DESTINATARIO_NOTIFICACION(SUB, ID_NEGOCIO, ID_TIPO_RECEPTOR, DESTINO, CODIGO_VALIDACION, FECHA_CADUCIDAD_CODIGO_VALIDACION, FECHA_VALIDACION, VALIDADO, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@SUB, @IDNEGOCIO, @IDTIPORECEPTOR, @DESTINO, @CODIGOVALIDACION, @FECHACADUCIDADCODIGOVALIDACION, @FECHAVALIDACION, @VALIDADO, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID",
				new { item.Sub, item.IdNegocio, item.IdTipoReceptor, item.Destino, item.CodigoValidacion, item.FechaCaducidadCodigoValidacion, item.FechaValidacion, item.Validado, item.FechaCreacion, item.FechaEliminacion, item.Vigencia }
			);
		}

		public async Task Actualizar(DestinatarioNotificacion item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.DESTINATARIO_NOTIFICACION SET SUB = @SUB, ID_NEGOCIO = @IDNEGOCIO, ID_TIPO_RECEPTOR = @IDTIPORECEPTOR, DESTINO = @DESTINO, " +
				"CODIGO_VALIDACION = @CODIGOVALIDACION, FECHA_CADUCIDAD_CODIGO_VALIDACION = @FECHACADUCIDADCODIGOVALIDACION, " +
				"FECHA_VALIDACION = @FECHAVALIDACION, VALIDADO = @VALIDADO, FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";
			DynamicParameters param = new();
			param.Add("SUB", item.Sub);
			param.Add("IDNEGOCIO", item.IdNegocio);
			param.Add("IDTIPORECEPTOR", item.IdTipoReceptor);
			param.Add("DESTINO", item.Destino);
			param.Add("CODIGOVALIDACION", item.CodigoValidacion);
			param.Add("FECHACADUCIDADCODIGOVALIDACION", item.FechaCaducidadCodigoValidacion);
			param.Add("FECHAVALIDACION", item.FechaValidacion);
			param.Add("VALIDADO", item.Validado);
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
