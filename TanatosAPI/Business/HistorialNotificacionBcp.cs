using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class HistorialNotificacionBcp(HistorialNotificacionDao historialNotificacionDao) {
		public async Task EliminarPorHistorialNormaSuscrita(HistorialNormaSuscrita historialNormaSuscrita, NpgsqlTransaction? transaction = null) {
			List<HistorialNotificacion> historialNotificacionesEliminar = await historialNotificacionDao.ObtenerPorHistorial(historialNormaSuscrita.Id, null, transaction);
			foreach (HistorialNotificacion historialNotificacionEliminar in historialNotificacionesEliminar) {
				await historialNotificacionDao.Eliminar(historialNotificacionEliminar.Id, transaction);
			}
		}
	}
}
