using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface ITipoActividadBcp {
		public bool EstaVigente(TipoActividad? item);
		public List<TipoActividad> FiltrarVigentes(List<TipoActividad> items);
		public Task<List<TipoActividad>> ObtenerTodos(bool filtrarVigentes = false, NpgsqlTransaction? transaction = null);
	}
}
