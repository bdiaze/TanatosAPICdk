using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface IHistorialNormaSuscritaDao {
		public Task<HistorialNormaSuscrita?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null);
		public Task<List<HistorialNormaSuscrita>> ObtenerPorNormaSuscrita(long idNormaSuscrita, bool? vigencia = true, NpgsqlTransaction? transaction = null);
		public Task<List<HistorialNormaSuscrita>> ObtenerPorNormaSuscritaYFechaCompletitud(long idNormaSuscrita, DateTime? fechaCompletitud, bool vigencia = true, NpgsqlTransaction? transaction = null);
		public Task<long> Insertar(HistorialNormaSuscrita item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(HistorialNormaSuscrita item, NpgsqlTransaction? transaction = null);
	}
}
