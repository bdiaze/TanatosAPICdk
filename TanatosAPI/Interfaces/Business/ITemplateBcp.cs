using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface ITemplateBcp {
		public bool EstaVigente(Template? template);
		public Task<Template?> Obtener(long idTemplate, bool filtrarVigente = false, NpgsqlTransaction? transaction = null);
		public Task<List<Template>> ObtenerVigentes(NpgsqlTransaction? transaction = null);
		public Task<List<Template>> ObtenerVariosSoloVigentes(HashSet<long> ids, NpgsqlTransaction? transaction = null);
	}
}
