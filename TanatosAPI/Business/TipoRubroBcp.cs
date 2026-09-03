using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class TipoRubroBcp(ITipoRubroDao tipoRubroDao) : ITipoRubroBcp {
		public bool EstaVigente(TipoRubro? item) {
			return item != null && item.Vigencia;
		}

		public List<TipoRubro> FiltrarVigentes(List<TipoRubro> items) {
			return [.. items.Where(ns => EstaVigente(ns))];
		}

		public async Task<List<TipoRubro>> ObtenerTodos(bool filtrarVigentes = false, NpgsqlTransaction? transaction = null) {
			List<TipoRubro> items = await tipoRubroDao.ObtenerPorVigencia(null, transaction);
			if (filtrarVigentes) items = FiltrarVigentes(items);
			return items;
		}
	}
}
