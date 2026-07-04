using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface ICargoDao {
		public Task<Cargo?> Obtener(long id, NpgsqlTransaction? transaction = null);

		public Task<List<Cargo>> ObtenerPorSub(string sub, long? idNegocio = null, bool? vigencia = true, NpgsqlTransaction? transaction = null);

		public Task<long> Insertar(Cargo item, NpgsqlTransaction? transaction = null);

		public Task Actualizar(Cargo item, NpgsqlTransaction? transaction = null);
	}
}
