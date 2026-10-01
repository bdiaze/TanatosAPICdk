using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface IAccesoDao {
		public Task<Acceso?> ObtenerPorHashCodigo(string hashCodigo, NpgsqlTransaction? transaction = null);
		public Task<long> Insertar(Acceso item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(Acceso item, NpgsqlTransaction? transaction = null);

	}
}
