using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface INormaSuscritaProcesoNotificacionDao {
		public Task<List<NormaSuscritaProcesoNotificacion>> ObtenerPorNormaSuscrita(long idNormaSuscrita, NpgsqlTransaction? transaction = null);
		public Task<long> Insertar(NormaSuscritaProcesoNotificacion item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(NormaSuscritaProcesoNotificacion item, NpgsqlTransaction? transaction = null);
	}
}
