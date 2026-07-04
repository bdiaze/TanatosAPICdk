using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface IPreguntaFrecuenteDao {
		public Task<List<PreguntaFrecuente>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null);
		public Task<long> Insertar(PreguntaFrecuente item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(PreguntaFrecuente item, NpgsqlTransaction? transaction = null);
	}
}
