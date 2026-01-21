using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class HistorialNotificacionBcp(HistorialNotificacionDao historialNotificacionDao) {
		public async Task EliminarPorHistorialNormaSuscrita(HistorialNormaSuscrita historialNormaSuscrita, NpgsqlTransaction? transaction = null) {
			List<HistorialNotificacion> historialNotificacionesEliminar = await historialNotificacionDao.ObtenerPorHistorial(historialNormaSuscrita.Id, null, true, transaction);
			foreach (HistorialNotificacion historialNotificacionEliminar in historialNotificacionesEliminar) {
				historialNotificacionEliminar.FechaEliminacion = DateTime.UtcNow;
				historialNotificacionEliminar.Vigencia = false;

				await historialNotificacionDao.Actualizar(historialNotificacionEliminar, transaction);
			}
		}

		public async Task EliminarPorHistorialNormaSuscritaYAntelacion(long idHistorialNormaSuscrita, long idTipoUnidadTiempoAntelacion, int cantAntelacion, NpgsqlTransaction? transaction = null) {
			List<HistorialNotificacion> historialNotificaciones = await historialNotificacionDao.ObtenerPorHistorial(idHistorialNormaSuscrita, null, true, transaction);
			foreach (HistorialNotificacion historialNotificacionEliminar in historialNotificaciones.Where(hne => hne.IdTipoUnidadTiempoAntelacion == idTipoUnidadTiempoAntelacion && hne.CantAntelacion == cantAntelacion)) {
				historialNotificacionEliminar.FechaEliminacion = DateTime.UtcNow;
				historialNotificacionEliminar.Vigencia = false;

				await historialNotificacionDao.Actualizar(historialNotificacionEliminar, transaction);
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
				FechaCreacion = DateTime.UtcNow,
				FechaEliminacion = null,
				Vigencia = true
			};

			nuevo.Id = await historialNotificacionDao.Insertar(nuevo, transaction);

			return nuevo;
		}
	}
}
