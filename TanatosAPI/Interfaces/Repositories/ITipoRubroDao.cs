using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface ITipoRubroDao {
		public Task<TipoRubro?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null);
		public Task<List<TipoRubro>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null);
		public Task Insertar(TipoRubro item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(TipoRubro item, NpgsqlTransaction? transaction = null);
		public Task Eliminar(long id, NpgsqlTransaction? transaction = null);
	}
}
