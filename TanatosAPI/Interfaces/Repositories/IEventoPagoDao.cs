using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface IEventoPagoDao {
		public Task<long> Insertar(EventoPago item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(EventoPago item, NpgsqlTransaction? transaction = null);
	}
}
