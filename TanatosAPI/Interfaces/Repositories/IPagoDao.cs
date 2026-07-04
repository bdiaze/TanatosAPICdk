using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface IPagoDao {
		public Task<Pago?> ObtenerPorFlow(string flowSubscriptionId, string flowInvoiceId, NpgsqlTransaction? transaction = null);
		public Task<long> Insertar(Pago item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(Pago item, NpgsqlTransaction? transaction = null);
	}
}
