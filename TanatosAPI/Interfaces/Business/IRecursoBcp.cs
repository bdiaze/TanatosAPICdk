using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface IRecursoBcp {
		public bool EstaVigente(Recurso? item);
		public List<Recurso> FiltrarVigentes(List<Recurso> items);
		public Task<List<Recurso>> ObtenerVarios(HashSet<long> ids, bool filtrarVigentes = false, NpgsqlTransaction? transaction = null);
		public Task<Recurso> Crear(string tipo, string idInterno, NpgsqlTransaction? transaction = null);
		public Task Eliminar(Recurso item, NpgsqlTransaction? transaction = null);
	}
}
