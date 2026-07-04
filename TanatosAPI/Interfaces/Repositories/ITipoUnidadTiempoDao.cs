using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface ITipoUnidadTiempoDao {
		public Task<TipoUnidadTiempo?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null);
		public Task<List<TipoUnidadTiempo>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null);
		public Task Insertar(TipoUnidadTiempo item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(TipoUnidadTiempo item, NpgsqlTransaction? transaction = null);
		public Task Eliminar(long id, NpgsqlTransaction? transaction = null);
	}
}
