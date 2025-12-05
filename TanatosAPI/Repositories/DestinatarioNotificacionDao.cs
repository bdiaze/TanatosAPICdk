using Dapper;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class DestinatarioNotificacionDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<List<DestinatarioNotificacion>> ObtenerPorSub(string sub, bool vigencia = true) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<DestinatarioNotificacion>(
				"SELECT ID, SUB, ID_TIPO_RECEPTOR, DESTINO, CODIGO_VALIDACION, INTENTOS_VALIDACION, VALIDADO, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.DESTINATARIO_NOTIFICACION WHERE SUB = @SUB AND VIGENCIA = @VIGENCIA",
				new { sub, vigencia }
			)];
		}

		public async Task<long> Insertar(DestinatarioNotificacion item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return await connection.ExecuteScalarAsync<long>(
				"INSERT INTO TANATOS.DESTINATARIO_NOTIFICACION(SUB, ID_TIPO_RECEPTOR, DESTINO, CODIGO_VALIDACION, INTENTOS_VALIDACION, VALIDADO, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@SUB, @IDTIPORECEPTOR, @DESTINO, @CODIGOVALIDACION, @INTENTOSVALIDACION, @VALIDADO, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID",
				new { item.Sub, item.IdTipoReceptor, item.Destino, item.CodigoValidacion, item.IntentosValidacion, item.Validado, item.FechaCreacion, item.FechaEliminacion, item.Vigencia }
			);
		}

		public async Task Actualizar(DestinatarioNotificacion item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"UPDATE TANATOS.DESTINATARIO_NOTIFICACION SET SUB = @SUB, ID_TIPO_RECEPTOR = @IDTIPORECEPTOR, DESTINO = @DESTINO, CODIGO_VALIDACION = @CODIGOVALIDACION, " +
				"INTENTOS_VALIDACION = @INTENTOSVALIDACION, VALIDADO = @VALIDADO, FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID",
				new { item.Sub, item.IdTipoReceptor, item.Destino, item.CodigoValidacion, item.IntentosValidacion, item.Validado, item.FechaCreacion, item.FechaEliminacion, item.Vigencia, item.Id }
			);
		}

		public async Task Eliminar(long id) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"DELETE FROM TANATOS.DESTINATARIO_NOTIFICACION WHERE ID = @ID",
				new { id }
			);
		}
	}
}
