using Dapper;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class TemplateNormaDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<List<TemplateNorma>> ObtenerPorSub(long idTemplate) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<TemplateNorma>(
				"SELECT ID, ID_TEMPLATE, NOMBRE, DESCRIPCION, ID_TIPO_PERIODICIDAD, MULTA, ID_CATEGORIA_NORMA FROM TANATOS.TEMPLATE_NORMA WHERE ID_TEMPLATE = @IDTEMPLATE",
				new { idTemplate }
			)];
		}

		public async Task Insertar(TemplateNorma item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"INSERT INTO TANATOS.TEMPLATE_NORMA(ID, ID_TEMPLATE, NOMBRE, DESCRIPCION, ID_TIPO_PERIODICIDAD, MULTA, ID_CATEGORIA_NORMA) " +
				"VALUES (@ID, @IDTEMPLATE, @NOMBRE, @DESCRIPCION, @IDTIPOPERIODICIDAD, @MULTA, @IDCATEGORIANORMA)",
				new { item.Id, item.IdTemplate, item.Nombre, item.Descripcion, item.IdTipoPeriodicidad, item.Multa, item.IdCategoriaNorma }
			);
		}

		public async Task Actualizar(TemplateNorma item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"UPDATE TANATOS.TEMPLATE_NORMA SET ID_TEMPLATE = @IDTEMPLATE, NOMBRE = @NOMBRE, DESCRIPCION = @DESCRIPCION, ID_TIPO_PERIODICIDAD = @IDTIPOPERIODICIDAD, " +
				"MULTA = @MULTA, ID_CATEGORIA_NORMA = @IDCATEGORIANORMA WHERE ID = @ID",
				new { item.IdTemplate, item.Nombre, item.Descripcion, item.IdTipoPeriodicidad, item.Multa, item.IdCategoriaNorma, item.Id }
			);
		}

		public async Task Eliminar(long id) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"DELETE FROM TANATOS.TEMPLATE_NORMA WHERE ID = @ID",
				new { id }
			);
		}
	}
}
