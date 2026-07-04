using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface INegocioDao {
		public Task<Negocio?> Obtener(long id, NpgsqlTransaction? transaction = null);
		public Task<List<Negocio>> ObtenerPorSub(string sub, bool vigencia = true, NpgsqlTransaction? transaction = null);
		public Task<long> Insertar(Negocio item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(Negocio item, NpgsqlTransaction? transaction = null);
	}
}
