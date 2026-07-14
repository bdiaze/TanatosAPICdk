using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface IEvaluacionDao {
		public Task<List<Evaluacion>> Obtener(DateTime? fechaDesde = null, DateTime? fechasHasta = null, NpgsqlTransaction? transaction = null);
		public Task<long> Insertar(Evaluacion item, NpgsqlTransaction? transaction = null);
	}
}
