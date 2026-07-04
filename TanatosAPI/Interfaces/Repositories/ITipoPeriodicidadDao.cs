using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface ITipoPeriodicidadDao {
		public Task<TipoPeriodicidad?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null);
		public Task<List<TipoPeriodicidad>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null);
		public Task Insertar(TipoPeriodicidad item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(TipoPeriodicidad item, NpgsqlTransaction? transaction = null);
		public Task Eliminar(long id, NpgsqlTransaction? transaction = null);
	}
}
