using Dapper;
using Npgsql;
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
			DynamicParameters param = new();
			param.Add("SUB", sub);
			param.Add("IDNEGOCIO", idNegocio);
			param.Add("VIGENCIA", vigencia);

			if (transaction?.Connection != null) {
				return [.. await transaction!.Connection!.QueryAsync<DestinatarioNotificacion>(query, param, transaction)];
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				return [.. await connection.QueryAsync<DestinatarioNotificacion>(query, param)];
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

		public async Task Actualizar(DestinatarioNotificacion item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"UPDATE TANATOS.DESTINATARIO_NOTIFICACION SET SUB = @SUB, ID_NEGOCIO = @IDNEGOCIO, ID_TIPO_RECEPTOR = @IDTIPORECEPTOR, DESTINO = @DESTINO, CODIGO_VALIDACION = @CODIGOVALIDACION, FECHA_CADUCIDAD_CODIGO_VALIDACION = @FECHACADUCIDADCODIGOVALIDACION, " +
				"FECHA_VALIDACION = @FECHAVALIDACION, VALIDADO = @VALIDADO, FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID",
				new { item.Sub, item.IdNegocio, item.IdTipoReceptor, item.Destino, item.CodigoValidacion, item.FechaCaducidadCodigoValidacion, item.FechaValidacion, item.Validado, item.FechaCreacion, item.FechaEliminacion, item.Vigencia, item.Id }
			);
		}
	}
}
