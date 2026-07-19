using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface ITemplateNormaBcp {
		public Task<List<TemplateNorma>> ObtenerPorTemplate(long idTemplate, NpgsqlTransaction? transaction = null);
		public Task<TemplateNorma?> ObtenerPorTemplateNorma(long idTemplate, long idNorma, NpgsqlTransaction? transaction = null);
	}
}
