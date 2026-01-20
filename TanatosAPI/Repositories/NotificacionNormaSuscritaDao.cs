using Dapper;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class NotificacionNormaSuscritaDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<List<NotificacionNormaSuscrita>> ObtenerPorNormaSuscrita(long idNormaSuscrita, bool vigencia = true, NpgsqlTransaction? transaction = null) {
			string query =
				"SELECT ID, ID_NORMA_SUSCRITA, ID_TIPO_UNIDAD_TIEMPO_ANTELACION, CANT_ANTELACION, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA FROM TANATOS.NOTIFICACION_NORMA_SUSCRITA " +
				"WHERE ID_NORMA_SUSCRITA = @IDNORMASUSCRITA AND (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)";
			DynamicParameters param = new();
			param.Add("IDNORMASUSCRITA", idNormaSuscrita);
			param.Add("VIGENCIA", vigencia);

			if (transaction?.Connection != null) {
				return [.. await transaction!.Connection!.QueryAsync<NotificacionNormaSuscrita>(query, param, transaction)];
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				return [.. await connection.QueryAsync<NotificacionNormaSuscrita>(query, param)];
			}
		}

		public async Task<long> Insertar(NotificacionNormaSuscrita item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.NOTIFICACION_NORMA_SUSCRITA(ID_NORMA_SUSCRITA, ID_TIPO_UNIDAD_TIEMPO_ANTELACION, CANT_ANTELACION, FECHA_CREACION, FECHA_ELIMINACION, VIGENCIA) " +
				"VALUES (@IDNORMASUSCRITA, @IDTIPOUNIDADTIEMPOANTELACION, @CANTANTELACION, @FECHACREACION, @FECHAELIMINACION, @VIGENCIA) " +
				"RETURNING ID";
			DynamicParameters param = new();
			param.Add("IDNORMASUSCRITA", item.IdNormaSuscrita);
			param.Add("IDTIPOUNIDADTIEMPOANTELACION", item.IdTipoUnidadTiempoAntelacion);
			param.Add("CANTANTELACION", item.CantAntelacion);
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

		public async Task Actualizar(NotificacionNormaSuscrita item, NpgsqlTransaction? transaction = null) {
			string query =
				"UPDATE TANATOS.NOTIFICACION_NORMA_SUSCRITA SET ID_NORMA_SUSCRITA = @IDNORMASUSCRITA, ID_TIPO_UNIDAD_TIEMPO_ANTELACION = @IDTIPOUNIDADTIEMPOANTELACION, CANT_ANTELACION = @CANTANTELACION, " +
				"FECHA_CREACION = @FECHACREACION, FECHA_ELIMINACION = @FECHAELIMINACION, VIGENCIA = @VIGENCIA " +
				"WHERE ID = @ID";
			DynamicParameters param = new();
			param.Add("IDNORMASUSCRITA", item.IdNormaSuscrita);
			param.Add("IDTIPOUNIDADTIEMPOANTELACION", item.IdTipoUnidadTiempoAntelacion);
			param.Add("CANTANTELACION", item.CantAntelacion);
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
