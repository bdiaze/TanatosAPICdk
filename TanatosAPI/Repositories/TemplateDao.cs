using Dapper;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class TemplateDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<Template?> ObtenerPorId(long idTemplate) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return await connection.QueryFirstOrDefaultAsync<Template>(
				"SELECT ID, ID_TEMPLATE_PADRE, NOMBRE, DESCRIPCION, REQUIERE_PLAN_EMPRESA, VIGENCIA FROM TANATOS.TEMPLATE WHERE ID = @IDTEMPLATE",
				new { idTemplate }
			);
		}

		public async Task<List<Template>> ObtenerPorVigencia(bool? vigencia) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<Template>(
				"SELECT ID, ID_TEMPLATE_PADRE, NOMBRE, DESCRIPCION, REQUIERE_PLAN_EMPRESA, VIGENCIA FROM TANATOS.TEMPLATE WHERE (VIGENCIA = @VIGENCIA OR @VIGENCIA IS NULL)",
				new { vigencia }
			)];
		}

		public async Task Insertar(Template item, NpgsqlTransaction? transaction = null) {
			string query = "INSERT INTO TANATOS.TEMPLATE(ID, ID_TEMPLATE_PADRE, NOMBRE, DESCRIPCION, REQUIERE_PLAN_EMPRESA, VIGENCIA) " +
				"VALUES (@ID, @IDTEMPLATEPADRE, @NOMBRE, @DESCRIPCION, @REQUIEREPLANEMPRESA, @VIGENCIA)";
			DynamicParameters param = new();
			param.Add("ID", item.Id);
			param.Add("IDTEMPLATEPADRE", item.IdTemplatePadre);
			param.Add("NOMBRE", item.Nombre);
			param.Add("DESCRIPCION", item.Descripcion);
			param.Add("REQUIEREPLANEMPRESA", item.RequierePlanEmpresa);
			param.Add("VIGENCIA", item.Vigencia);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}

		public async Task Actualizar(Template item, NpgsqlTransaction? transaction = null) {
			string query = "UPDATE TANATOS.TEMPLATE SET ID_TEMPLATE_PADRE = @IDTEMPLATEPADRE, NOMBRE = @NOMBRE, DESCRIPCION = @DESCRIPCION, " +
				"REQUIERE_PLAN_EMPRESA = @REQUIEREPLANEMPRESA, VIGENCIA = @VIGENCIA WHERE ID = @ID";
			DynamicParameters param = new();
			param.Add("IDTEMPLATEPADRE", item.IdTemplatePadre);
			param.Add("NOMBRE", item.Nombre);
			param.Add("DESCRIPCION", item.Descripcion);
			param.Add("REQUIEREPLANEMPRESA", item.RequierePlanEmpresa);
			param.Add("VIGENCIA", item.Vigencia);
			param.Add("ID", item.Id);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}

		public async Task Eliminar(long id, NpgsqlTransaction? transaction = null) {
			string query = "DELETE FROM TANATOS.TEMPLATE WHERE ID = @ID";
			DynamicParameters param = new();
			param.Add("ID", id);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}
	}
}
