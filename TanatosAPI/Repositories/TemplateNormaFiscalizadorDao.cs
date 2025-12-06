using Dapper;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class TemplateNormaFiscalizadorDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<List<TemplateNormaFiscalizador>> ObtenerPorTemplateNorma(long idTemplate, long? idNorma = null) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<TemplateNormaFiscalizador>(
				"SELECT ID_TEMPLATE, ID_NORMA, ID_TIPO_FISCALIZADOR FROM TANATOS.TEMPLATE_NORMA_FISCALIZADOR WHERE ID_TEMPLATE = @IDTEMPLATE AND (ID_NORMA = @IDNORMA OR @IDNORMA IS NULL)",
				new { idTemplate, idNorma }
			)];
		}

		public async Task<List<TemplateNormaFiscalizador>> ObtenerPorFiscalizador(long idTipoFiscalizador) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<TemplateNormaFiscalizador>(
				"SELECT ID_TEMPLATE, ID_NORMA, ID_TIPO_FISCALIZADOR FROM TANATOS.TEMPLATE_NORMA_FISCALIZADOR WHERE ID_TIPO_FISCALIZADOR = @IDTIPOFISCALIZADOR",
				new { idTipoFiscalizador }
			)];
		}

		public async Task Insertar(TemplateNormaFiscalizador item, NpgsqlTransaction? transaction = null) {
			string query = "INSERT INTO TANATOS.TEMPLATE_NORMA_FISCALIZADOR(ID_TEMPLATE, ID_NORMA, ID_TIPO_FISCALIZADOR) VALUES (@IDTEMPLATE, @IDNORMA, @IDTIPOFISCALIZADOR)";
			DynamicParameters param = new();
			param.Add("IDTEMPLATE", item.IdTemplate);
			param.Add("IDNORMA", item.IdNorma);
			param.Add("IDTIPOFISCALIZADOR", item.IdTipoFiscalizador);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}

		public async Task Eliminar(long idTemplate, long? idNorma, long? idTipoFiscalizador, NpgsqlTransaction? transaction = null) {
			string query = "DELETE FROM TANATOS.TEMPLATE_NORMA_FISCALIZADOR WHERE ID_TEMPLATE = @IDTEMPLATE AND (ID_NORMA = @IDNORMA OR @IDNORMA IS NULL) AND (ID_TIPO_FISCALIZADOR = @IDTIPOFISCALIZADOR OR @IDTIPOFISCALIZADOR IS NULL)";
			DynamicParameters param = new();
			param.Add("IDTEMPLATE", idTemplate);
			param.Add("IDNORMA", idNorma);
			param.Add("IDTIPOFISCALIZADOR", idTipoFiscalizador);

			if (transaction?.Connection != null) {
				await transaction!.Connection!.ExecuteAsync(query, param, transaction);
			} else {
				await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
				await connection.ExecuteAsync(query, param);
			}
		}
	}
}
