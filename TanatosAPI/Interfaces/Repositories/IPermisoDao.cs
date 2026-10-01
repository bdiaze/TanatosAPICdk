using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface IPermisoDao {
		public Task<List<Permiso>> ObtenerPorIdAcceso(long idAcceso, NpgsqlTransaction? transaction = null);
		public Task<long> Insertar(Permiso item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(Permiso item, NpgsqlTransaction? transaction = null);
	}
}
