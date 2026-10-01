using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface IRecursoDao {
		public Task<List<Recurso>> ObtenerVarios(HashSet<long> ids, NpgsqlTransaction? transaction = null);
		public Task<long> Insertar(Recurso item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(Recurso item, NpgsqlTransaction? transaction = null);
	}
}
