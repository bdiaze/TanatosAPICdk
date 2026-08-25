using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface ITipoProcesoAutomaticoDao {
		public Task<TipoProcesoAutomatico?> Obtener(long id, NpgsqlTransaction? transaction = null);
		public Task<List<TipoProcesoAutomatico>> ObtenerTodos(NpgsqlTransaction? transaction = null);
		public Task<long> Insertar(TipoProcesoAutomatico item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(TipoProcesoAutomatico item, NpgsqlTransaction? transaction = null);
	}
}
