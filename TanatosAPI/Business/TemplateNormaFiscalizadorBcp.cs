using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class TemplateNormaFiscalizadorBcp(ITemplateNormaFiscalizadorDao templateNormaFiscalizadorDao) : ITemplateNormaFiscalizadorBcp {
		public async Task<List<TemplateNormaFiscalizador>> ObtenerPorTemplateNorma(long idTemplate, long? idNorma = null, NpgsqlTransaction? transaction = null) {
			return await templateNormaFiscalizadorDao.ObtenerPorTemplateNorma(idTemplate, idNorma, transaction);
		}
	}
}
