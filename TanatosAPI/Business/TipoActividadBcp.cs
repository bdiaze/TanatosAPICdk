using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Business {
	public class TipoActividadBcp(ITipoActividadDao tipoActividadDao) : ITipoActividadBcp {
		public bool EstaVigente(TipoActividad? item) {
			return item != null && item.Vigencia;
		}

		public List<TipoActividad> FiltrarVigentes(List<TipoActividad> items) {
			return [.. items.Where(ns => EstaVigente(ns))];
		}

		public async Task<List<TipoActividad>> ObtenerTodos(bool filtrarVigentes = false, NpgsqlTransaction? transaction = null) {
			List<TipoActividad> items = await tipoActividadDao.ObtenerPorVigencia(null, transaction);
			if (filtrarVigentes) items = FiltrarVigentes(items);
			return items;
		}
	}
}
