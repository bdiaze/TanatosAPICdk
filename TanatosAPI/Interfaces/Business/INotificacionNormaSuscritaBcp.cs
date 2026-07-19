using Npgsql;
using TanatosAPI.Entities.Models;

namespace TanatosAPI.Interfaces.Business {
	public interface INotificacionNormaSuscritaBcp {
		public Task<List<NotificacionNormaSuscrita>> ObtenerVigentesPorNormaSuscrita(long idNormaSuscrita, NpgsqlTransaction? transaction = null);
		public Task Eliminar(NotificacionNormaSuscrita notificacionNormaSuscrita, NpgsqlTransaction? transaction = null);
		public Task EliminarPorNormaSuscrita(long idNormaSuscrita, NpgsqlTransaction? transaction = null);
		public Task ActualizarPorNormaSuscrita(long idNormaSuscrita, HashSet<(long IdTipoUnidadTiempoAntelacion, int CantAntelacion)> notificacionesNormaSuscrita, NpgsqlTransaction? transaction = null);
	}
}
