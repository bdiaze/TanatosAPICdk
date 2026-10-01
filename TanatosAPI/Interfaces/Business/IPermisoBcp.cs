using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface IPermisoBcp {
		public bool EstaVigente(Permiso? item);
		public List<Permiso> FiltrarVigentes(List<Permiso> items);
		public Task<List<Permiso>> ObtenerPorIdAcceso(long idAcceso, bool filtrarVigentes = false, NpgsqlTransaction? transaction = null);
		public Task<Permiso> Crear(long idAcceso, long idRecurso, string accion, NpgsqlTransaction? transaction = null);
		public Task Eliminar(Permiso item, NpgsqlTransaction? transaction = null);
	}
}
