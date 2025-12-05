using Dapper;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class TemplateNormaNotificacionDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<List<TemplateNormaNotificacion>> ObtenerPorTemplateNorma(long idTemplateNorma) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<TemplateNormaNotificacion>(
				"SELECT ID_TEMPLATE_NORMA, ID_TIPO_UNIDAD_TIEMPO_ANTELACION, CANT_ANTELACION FROM TANATOS.TEMPLATE_NORMA_NOTIFICACION WHERE ID_TEMPLATE_NORMA = @IDTEMPLATENORMA",
				new { idTemplateNorma }
			)];
		}

		public async Task<List<TemplateNormaNotificacion>> ObtenerPorTipoUnidadTiempoAntelacion(long idTipoUnidadTiempoAntelacion) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<TemplateNormaNotificacion>(
				"SELECT ID_TEMPLATE_NORMA, ID_TIPO_UNIDAD_TIEMPO_ANTELACION, CANT_ANTELACION FROM TANATOS.TEMPLATE_NORMA_NOTIFICACION WHERE ID_TIPO_UNIDAD_TIEMPO_ANTELACION = @IDTIPOUNIDADTIEMPOANTELACION",
				new { idTipoUnidadTiempoAntelacion }
			)];
		}

		public async Task Insertar(TemplateNormaNotificacion item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"INSERT INTO TANATOS.TEMPLATE_NORMA_NOTIFICACION(ID_TEMPLATE_NORMA, ID_TIPO_UNIDAD_TIEMPO_ANTELACION, CANT_ANTELACION) " +
				"VALUES (@IDTEMPLATENORMA, @IDTIPOUNIDADTIEMPOANTELACION, @CANTANTELACION)",
				new { item.IdTemplateNorma, item.IdTipoUnidadTiempoAntelacion, item.CantAntelacion }
			);
		}

		public async Task Actualizar(TemplateNormaNotificacion item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"UPDATE TANATOS.TEMPLATE_NORMA_NOTIFICACION SET CANT_ANTELACION = @CANTANTELACION WHERE ID_TEMPLATE_NORMA = @IDTEMPLATENORMA AND ID_TIPO_UNIDAD_TIEMPO_ANTELACION = @IDTIPOUNIDADTIEMPOANTELACION",
				new { item.CantAntelacion, item.IdTemplateNorma, item.IdTipoUnidadTiempoAntelacion }
			);
		}

		public async Task Eliminar(long idTemplateNorma, long idTipoUnidadTiempoAntelacion) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"DELETE FROM TANATOS.TEMPLATE_NORMA_NOTIFICACION WHERE ID_TEMPLATE_NORMA = @IDTEMPLATENORMA AND ID_TIPO_UNIDAD_TIEMPO_ANTELACION = @IDTIPOUNIDADTIEMPOANTELACION",
				new { idTemplateNorma, idTipoUnidadTiempoAntelacion }
			);
		}
	}
}
