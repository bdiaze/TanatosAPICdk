using Dapper;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class TemplateDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<List<Template>> ObtenerPorVigencia(bool vigencia) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<Template>(
				"SELECT ID, ID_TEMPLATE_PADRE, NOMBRE, DESCRIPCION, VIGENCIA FROM TANATOS.TEMPLATE WHERE VIGENCIA = @VIGENCIA",
				new { vigencia }
			)];
		}

		public async Task Insertar(Template item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"INSERT INTO TANATOS.TEMPLATE(ID, ID_TEMPLATE_PADRE, NOMBRE, DESCRIPCION, VIGENCIA) VALUES (@ID, @IDTEMPLATEPADRE, @NOMBRE, @DESCRIPCION, @VIGENCIA)",
				new { item.Id, item.IdTemplatePadre, item.Nombre, item.Descripcion, item.Vigencia }
			);
		}

		public async Task Actualizar(Template item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"UPDATE TANATOS.TEMPLATE SET ID_TEMPLATE_PADRE = @IDTEMPLATEPADRE, NOMBRE = @NOMBRE, DESCRIPCION = @DESCRIPCION, VIGENCIA = @VIGENCIA WHERE ID = @ID",
				new { item.IdTemplatePadre, item.Nombre, item.Descripcion, item.Vigencia, item.Id }
			);
		}

		public async Task Eliminar(long id) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"DELETE FROM TANATOS.TEMPLATE WHERE ID = @ID",
				new { id }
			);
		}
	}
}
