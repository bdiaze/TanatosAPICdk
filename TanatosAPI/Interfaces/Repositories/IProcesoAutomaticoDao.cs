using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface IProcesoAutomaticoDao {
		public Task<List<ProcesoAutomatico>> ObtenerVarios(HashSet<long> ids, NpgsqlTransaction? transaction = null);
		public Task<List<ProcesoAutomatico>> ObtenerConPaginacion(long? primerId = null, int cantidad = 50, string? nombre = null, bool? vigencia = true, NpgsqlTransaction? transaction = null);
		public Task<List<ProcesoAutomatico>> ObtenerPorNombre(string nombre, NpgsqlTransaction? transaction = null);
		public Task<long> Insertar(ProcesoAutomatico item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(ProcesoAutomatico item, NpgsqlTransaction? transaction = null);
	}
}
