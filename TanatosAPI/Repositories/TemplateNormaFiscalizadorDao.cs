using Dapper;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;

namespace TanatosAPI.Repositories {
	[DapperAot]
	public class TemplateNormaFiscalizadorDao(DatabaseConnectionHelper connectionHelper) {
		public async Task<List<TemplateNormaFiscalizador>> ObtenerPorTemplateNorma(long idTemplateNorma) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<TemplateNormaFiscalizador>(
				"SELECT ID_TEMPLATE_NORMA, ID_TIPO_FISCALIZADOR FROM TANATOS.TEMPLATE_NORMA_FISCALIZADOR WHERE ID_TEMPLATE_NORMA = @IDTEMPLATENORMA",
				new { idTemplateNorma }
			)];
		}

		public async Task<List<TemplateNormaFiscalizador>> ObtenerPorFiscalizador(long idTipoFiscalizador) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			return [.. await connection.QueryAsync<TemplateNormaFiscalizador>(
				"SELECT ID_TEMPLATE_NORMA, ID_TIPO_FISCALIZADOR FROM TANATOS.TEMPLATE_NORMA_FISCALIZADOR WHERE ID_TIPO_FISCALIZADOR = @IDTIPOFISCALIZADOR",
				new { idTipoFiscalizador }
			)];
		}

		public async Task Insertar(TemplateNormaFiscalizador item) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"INSERT INTO TANATOS.TEMPLATE_NORMA_FISCALIZADOR(ID_TEMPLATE_NORMA, ID_TIPO_FISCALIZADOR) VALUES (@IDTEMPLATENORMA, @IDTIPOFISCALIZADOR)",
				new { item.IdTemplateNorma, item.IdTipoFiscalizador }
			);
		}

		public async Task Eliminar(long idTemplateNorma, long idTipoFiscalizador) {
			await using NpgsqlConnection connection = await connectionHelper.ObtenerConexion();
			await connection.ExecuteAsync(
				"DELETE FROM TANATOS.TEMPLATE_NORMA_FISCALIZADOR WHERE ID_TEMPLATE_NORMA = @IDTEMPLATENORMA AND ID_TIPO_FISCALIZADOR = @IDTIPOFISCALIZADOR",
				new { idTemplateNorma, idTipoFiscalizador }
			);
		}
	}
}
