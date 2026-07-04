using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface ITipoActividadDao {
		public Task<TipoActividad?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null);
		public Task<List<TipoActividad>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null);
		public Task Insertar(TipoActividad item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(TipoActividad item, NpgsqlTransaction? transaction = null);
		public Task Eliminar(long id, NpgsqlTransaction? transaction = null);
	}
}
