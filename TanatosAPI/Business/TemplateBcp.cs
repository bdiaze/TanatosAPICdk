using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Business {
	public class TemplateBcp(ITemplateDao templateDao) : ITemplateBcp {
		public bool EstaVigente(Template? template) {
			return template != null && template.Vigencia;
		}

		public async Task<Template?> Obtener(long idTemplate, bool filtrarVigente = false, NpgsqlTransaction? transaction = null) {
			Template? template = await templateDao.ObtenerPorId(idTemplate, transaction);			
			if (filtrarVigente && !EstaVigente(template)) return null;			
			return template;
		}

		public async Task<List<Template>> ObtenerVigentes(NpgsqlTransaction? transaction = null) {
			return await templateDao.ObtenerPorVigencia(true, transaction);
		}

		public async Task<List<Template>> ObtenerVariosSoloVigentes(HashSet<long> ids, NpgsqlTransaction? transaction = null) {
			if (ids.Count == 0) return [];
			List<Template> vigentes = await ObtenerVigentes(transaction);
			return [.. vigentes.Where(t => ids.Contains(t.Id))];
		}
	}
}
