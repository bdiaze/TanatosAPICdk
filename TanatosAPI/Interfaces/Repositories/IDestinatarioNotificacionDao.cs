using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface IDestinatarioNotificacionDao {
		public Task<DestinatarioNotificacion?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null);
		public Task<DestinatarioNotificacion?> ObtenerPorCodigoValidacion(string codigoValidacion, NpgsqlTransaction? transaction = null);
		public Task<List<DestinatarioNotificacion>> ObtenerPorSub(string sub, long? idNegocio = null, bool? vigencia = true, NpgsqlTransaction? transaction = null);
		public Task<long> Insertar(DestinatarioNotificacion item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(DestinatarioNotificacion item, NpgsqlTransaction? transaction = null);
	}
}
