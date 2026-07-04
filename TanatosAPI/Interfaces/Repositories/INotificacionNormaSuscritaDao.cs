using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Repositories {
	public interface INotificacionNormaSuscritaDao {
		public Task<List<NotificacionNormaSuscrita>> ObtenerPorNormaSuscrita(long idNormaSuscrita, bool? vigencia = true, NpgsqlTransaction? transaction = null);
		public Task<long> Insertar(NotificacionNormaSuscrita item, NpgsqlTransaction? transaction = null);
		public Task Actualizar(NotificacionNormaSuscrita item, NpgsqlTransaction? transaction = null);
	}
}
