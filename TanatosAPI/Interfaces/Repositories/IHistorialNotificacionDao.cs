using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface IHistorialNotificacionDao {
		public Task<List<HistorialNotificacion>> ObtenerPorHistorial(long idHistorialNormaSuscrita, DateTime? fechaEjecucion = null, bool? vigencia = true, NpgsqlTransaction? transaction = null);
		public Task<HistorialNotificacion?> ObtenerPorCodigoAcceso(string codigoAcceso, bool? vigencia = true, NpgsqlTransaction? transaction = null);
		public Task<long> Insertar(HistorialNotificacion item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(HistorialNotificacion item, NpgsqlTransaction? transaction = null);
	}
}
