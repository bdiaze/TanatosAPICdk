using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface IMensajeDao {
		public Task<List<Mensaje>> ObtenerPorRangoFechas(DateTime? fechaInicial, DateTime? fechaFinal, NpgsqlTransaction? transaction = null);
		public Task<long> Insertar(Mensaje item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(Mensaje item, NpgsqlTransaction? transaction = null);
	}
}
