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

		public async Task EliminarPorHistorialNormaSuscritaYAntelacion(long idHistorialNormaSuscrita, long idTipoUnidadTiempoAntelacion, int cantAntelacion, NpgsqlTransaction? transaction = null) {
			List<HistorialNotificacion> historialNotificaciones = await historialNotificacionDao.ObtenerPorHistorial(idHistorialNormaSuscrita, null, transaction);
			foreach (HistorialNotificacion historialNotificacionEliminar in historialNotificaciones.Where(hne => hne.IdTipoUnidadTiempoAntelacion == idTipoUnidadTiempoAntelacion && hne.CantAntelacion == cantAntelacion)) {
				await historialNotificacionDao.Eliminar(historialNotificacionEliminar.Id, transaction);
			}
		}

		public async Task<HistorialNotificacion> CrearPorHistorialNormaSuscritaYAntelacion(long idHistorialNormaSuscrita, long idDestinatarioNotificacion, long? idTipoUnidadTiempoAntelacion, int? cantAntelacion, DateTime fechaProgramacion, NpgsqlTransaction? transaction = null) {
			HistorialNotificacion nuevo = new() {
				Id = 0,
				IdHistorialNormaSuscrita = idHistorialNormaSuscrita,
				IdDestinatarioNotificacion = idDestinatarioNotificacion,
				IdTipoUnidadTiempoAntelacion = idTipoUnidadTiempoAntelacion,
				CantAntelacion = cantAntelacion,
				FechaProgramacion = fechaProgramacion,
			};

			nuevo.Id = await historialNotificacionDao.Insertar(nuevo, transaction);

			return nuevo;
		}
	}
}
