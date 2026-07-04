using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface IPagoBcp {
		public Task<Pago?> ObtenerPorFlow(string flowSubscriptionId, string flowInvoiceId, NpgsqlTransaction? transaction = null);
		public Task<Pago> Insertar(string sub, long idSuscripcion, decimal monto, string moneda, DateTime fechaPago, string flowSubscriptionId, string flowInvoiceId, NpgsqlTransaction? transaction = null);
	}
}
