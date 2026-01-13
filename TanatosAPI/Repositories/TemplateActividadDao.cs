using Dapper;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class TemplateActividadDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<List<TemplateActividad>> ObtenerPorTemplate(long idTemplate) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<TemplateActividad>(
				"SELECT ID_TEMPLATE, ID_TIPO_ACTIVIDAD FROM TANATOS.TEMPLATE_ACTIVIDAD WHERE ID_TEMPLATE = @IDTEMPLATE",
				new { idTemplate }
			)];
		}

		public async Task<List<TemplateActividad>> ObtenerPorActividad(long idTipoActividad) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<TemplateActividad>(
				"SELECT ID_TEMPLATE, ID_TIPO_ACTIVIDAD FROM TANATOS.TEMPLATE_ACTIVIDAD WHERE ID_TIPO_ACTIVIDAD = @IDTIPOACTIVIDAD",
				new { idTipoActividad }
			)];
		}

		public async Task Insertar(TemplateActividad item, NpgsqlTransaction? transaction = null) {
			string query =
				"INSERT INTO TANATOS.TEMPLATE_ACTIVIDAD(ID_TEMPLATE, ID_TIPO_ACTIVIDAD) " +
				"VALUES (@IDTEMPLATE, @IDTIPOACTIVIDAD)";
			DynamicParameters param = new();
			param.Add("IDTEMPLATE", item.IdTemplate);
			param.Add("IDTIPOACTIVIDAD", item.IdTipoActividad);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}

		public async Task Eliminar(long idTemplate, long? idTipoActividad, NpgsqlTransaction? transaction = null) {
			string query = "DELETE FROM TANATOS.TEMPLATE_ACTIVIDAD WHERE ID_TEMPLATE = @IDTEMPLATE AND (ID_TIPO_ACTIVIDAD = @IDTIPOACTIVIDAD OR @IDTIPOACTIVIDAD IS NULL)";
			DynamicParameters param = new();
			param.Add("IDTEMPLATE", idTemplate);
			param.Add("IDTIPOACTIVIDAD", idTipoActividad);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}
	}
}
