using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class HistorialNotificacionBcp(HistorialNotificacionDao historialNotificacionDao) {
		public async Task EliminarPorHistorialNormaSuscrita(long idHistorialNormaSuscrita, NpgsqlTransaction? transaction = null) {
			List<HistorialNotificacion> historialNotificacionesEliminar = await historialNotificacionDao.ObtenerPorHistorial(idHistorialNormaSuscrita, null, true, transaction);
			foreach (HistorialNotificacion historialNotificacionEliminar in historialNotificacionesEliminar) {
				await Eliminar(historialNotificacionEliminar, transaction);
			}
		}

		public async Task EliminarPorHistorialNormaSuscritaYAntelacion(long idHistorialNormaSuscrita, long idTipoUnidadTiempoAntelacion, int cantAntelacion, NpgsqlTransaction? transaction = null) {
			List<HistorialNotificacion> historialNotificaciones = await historialNotificacionDao.ObtenerPorHistorial(idHistorialNormaSuscrita, null, true, transaction);
			foreach (HistorialNotificacion historialNotificacionEliminar in historialNotificaciones.Where(hne => hne.IdTipoUnidadTiempoAntelacion == idTipoUnidadTiempoAntelacion && hne.CantAntelacion == cantAntelacion)) {
				await Eliminar(historialNotificacionEliminar, transaction);
			}
		}

		public async Task<HistorialNotificacion?> Crear(long idHistorialNormaSuscrita, long idDestinatarioNotificacion, long? idTipoUnidadTiempoAntelacion, int? cantAntelacion, DateTime fechaProgramacion, NpgsqlTransaction? transaction = null) {
			if (fechaProgramacion > DateTime.UtcNow) {
				HistorialNotificacion nuevo = new() {
					Id = 0,
					IdHistorialNormaSuscrita = idHistorialNormaSuscrita,
					IdDestinatarioNotificacion = idDestinatarioNotificacion,
					IdTipoUnidadTiempoAntelacion = idTipoUnidadTiempoAntelacion,
					CantAntelacion = cantAntelacion,
					FechaProgramacion = fechaProgramacion,
					Estado = 0, // Pendiente
					FechaCreacion = DateTime.UtcNow,
					FechaEliminacion = null,
					Vigencia = true
				};

				nuevo.Id = await historialNotificacionDao.Insertar(nuevo, transaction);

				return nuevo;
			} else {
				return null;
			}
		}

		public async Task Eliminar(HistorialNotificacion historialNotificacion, NpgsqlTransaction? transaction = null) {
			if (historialNotificacion.Vigencia) {
				historialNotificacion.FechaEliminacion = DateTime.UtcNow;
				historialNotificacion.Vigencia = false;

				await historialNotificacionDao.Actualizar(historialNotificacion, transaction);
			}
		}
	}
}
