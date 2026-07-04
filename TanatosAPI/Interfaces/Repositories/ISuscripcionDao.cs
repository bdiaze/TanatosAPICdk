using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface ISuscripcionDao {
		public Task<Suscripcion?> Obtener(long id, NpgsqlTransaction? transaction = null);
		public Task<Suscripcion?> ObtenerPorFlowSubscriptionId(string flowSubscriptionId, NpgsqlTransaction? transaction = null);
		public Task<List<Suscripcion>> ObtenerPorSub(string sub, bool? vigencia = true, NpgsqlTransaction? transaction = null);
		public Task<long> Insertar(Suscripcion item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(Suscripcion item, NpgsqlTransaction? transaction = null);
	}
}
