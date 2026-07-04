using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface IEventoPagoBcp {
		public Task<EventoPago> Insertar(string proveedor, string evento, string payload, NpgsqlTransaction? transaction = null);
		public Task MarcarComoProcesado(EventoPago eventoPago, NpgsqlTransaction? transaction = null);
	}
}
