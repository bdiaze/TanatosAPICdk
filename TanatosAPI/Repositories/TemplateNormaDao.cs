using Dapper;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class TemplateNormaDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<List<TemplateNorma>> ObtenerPorTemplate(long idTemplate) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<TemplateNorma>(
				"SELECT ID_TEMPLATE, ID_NORMA, NOMBRE, DESCRIPCION, ID_TIPO_PERIODICIDAD, MULTA, ID_CATEGORIA_NORMA FROM TANATOS.TEMPLATE_NORMA WHERE ID_TEMPLATE = @IDTEMPLATE",
				new { idTemplate }
			)];
		}

		public async Task Insertar(TemplateNorma item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.TEMPLATE_NORMA(ID_TEMPLATE, ID_NORMA, NOMBRE, DESCRIPCION, ID_TIPO_PERIODICIDAD, MULTA, ID_CATEGORIA_NORMA) " +
				"VALUES (@IDTEMPLATE, @IDNORMA, @NOMBRE, @DESCRIPCION, @IDTIPOPERIODICIDAD, @MULTA, @IDCATEGORIANORMA)";
			DynamicParameters param = new();
			param.Add("IDTEMPLATE", item.IdTemplate);
			param.Add("IDNORMA", item.IdNorma);
			param.Add("NOMBRE", item.Nombre);
			param.Add("DESCRIPCION", item.Descripcion);
			param.Add("IDTIPOPERIODICIDAD", item.IdTipoPeriodicidad);
			param.Add("MULTA", item.Multa);
			param.Add("IDCATEGORIANORMA", item.IdCategoriaNorma);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}

		public async Task Actualizar(TemplateNorma item, NpgsqlTransaction? transaction = null) {
			string query = 
				"UPDATE TANATOS.TEMPLATE_NORMA SET NOMBRE = @NOMBRE, DESCRIPCION = @DESCRIPCION, ID_TIPO_PERIODICIDAD = @IDTIPOPERIODICIDAD, " +
				"MULTA = @MULTA, ID_CATEGORIA_NORMA = @IDCATEGORIANORMA WHERE ID_TEMPLATE = @IDTEMPLATE AND ID_NORMA = @IDNORMA";
			DynamicParameters param = new();
			param.Add("NOMBRE", item.Nombre);
			param.Add("DESCRIPCION", item.Descripcion);
			param.Add("IDTIPOPERIODICIDAD", item.IdTipoPeriodicidad);
			param.Add("MULTA", item.Multa);
			param.Add("IDCATEGORIANORMA", item.IdCategoriaNorma);
			param.Add("IDTEMPLATE", item.IdTemplate);
			param.Add("IDNORMA", item.IdNorma);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}

		public async Task Eliminar(long idTemplate, long? idNorma, NpgsqlTransaction? transaction = null) {
			string query = "DELETE FROM TANATOS.TEMPLATE_NORMA WHERE ID_TEMPLATE = @IDTEMPLATE AND (ID_NORMA = @IDNORMA OR @IDNORMA IS NULL)";
			DynamicParameters param = new();
			param.Add("IDTEMPLATE", idTemplate);
			param.Add("IDNORMA", idNorma);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}
	}
}
