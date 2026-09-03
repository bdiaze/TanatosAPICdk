using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface ITipoRubroBcp {
		public bool EstaVigente(TipoRubro? item);
		public List<TipoRubro> FiltrarVigentes(List<TipoRubro> items);
		public Task<List<TipoRubro>> ObtenerTodos(bool filtrarVigentes = false, NpgsqlTransaction? transaction = null);
	}
}
