using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface ITipoReceptorNotificacionDao {
		public Task<TipoReceptorNotificacion?> ObtenerPorId(long id, NpgsqlTransaction? transaction = null);
		public Task<List<TipoReceptorNotificacion>> ObtenerPorVigencia(bool? vigencia, NpgsqlTransaction? transaction = null);
		public Task Insertar(TipoReceptorNotificacion item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(TipoReceptorNotificacion item, NpgsqlTransaction? transaction = null);
		public Task Eliminar(long id, NpgsqlTransaction? transaction = null);
	}
}
