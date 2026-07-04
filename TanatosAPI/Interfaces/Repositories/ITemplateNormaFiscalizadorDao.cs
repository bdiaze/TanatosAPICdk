using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface ITemplateNormaFiscalizadorDao {
		public Task<List<TemplateNormaFiscalizador>> ObtenerPorTemplateNorma(long idTemplate, long? idNorma = null, NpgsqlTransaction? transaction = null);
		public Task<List<TemplateNormaFiscalizador>> ObtenerPorFiscalizador(long idTipoFiscalizador, NpgsqlTransaction? transaction = null);
		public Task Insertar(TemplateNormaFiscalizador item, NpgsqlTransaction? transaction = null);
		public Task Eliminar(long idTemplate, long? idNorma, long? idTipoFiscalizador, NpgsqlTransaction? transaction = null);
	}
}
