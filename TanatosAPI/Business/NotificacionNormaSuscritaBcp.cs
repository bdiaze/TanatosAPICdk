using Amazon.Lambda.Core;
using Npgsql;
using System.Diagnostics;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class NotificacionNormaSuscritaBcp(NotificacionNormaSuscritaDao notificacionNormaSuscritaDao, HistorialNormaSuscritaBcp historialNormaSuscritaBcp, HistorialNotificacionDao historialNotificacionDao) {
		public async Task ActualizarPorNormaSuscrita(NormaSuscrita normaSuscrita, List<NotificacionNormaSuscrita> notificacionesNormaSuscrita, NpgsqlTransaction? transaction = null) {
			List<NotificacionNormaSuscrita> notificacionesExistentes = await notificacionNormaSuscritaDao.ObtenerPorNormaSuscrita(normaSuscrita.Id, true, transaction);

			// Se eliminan las notificaciones existentes que no se incluyen en la entrada...
			foreach (NotificacionNormaSuscrita notificacionExistente in notificacionesExistentes) {
				if (!notificacionesNormaSuscrita.Any(n => n.IdTipoUnidadTiempoAntelacion == notificacionExistente.IdTipoUnidadTiempoAntelacion && n.CantAntelacion == notificacionExistente.CantAntelacion)) {
					await Eliminar(notificacionExistente, transaction);
				}
			}

			// Se agregan las nuevas notificaciones...
			foreach (NotificacionNormaSuscrita notificacionNueva in notificacionesNormaSuscrita) {
				if (!notificacionesExistentes.Any(ne => ne.IdTipoUnidadTiempoAntelacion == notificacionNueva.IdTipoUnidadTiempoAntelacion && ne.CantAntelacion == notificacionNueva.CantAntelacion)) {
					await Crear(normaSuscrita.Id, notificacionNueva.IdTipoUnidadTiempoAntelacion, notificacionNueva.CantAntelacion, transaction);
				}
			}
		}
		
		public async Task EliminarPorNormaSuscrita(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null) {
			List<NotificacionNormaSuscrita> notificacionesVigentes = await notificacionNormaSuscritaDao.ObtenerPorNormaSuscrita(normaSuscrita.Id, true);
			foreach (NotificacionNormaSuscrita notificacion in notificacionesVigentes) {
				notificacion.FechaEliminacion = DateTime.UtcNow;
				notificacion.Vigencia = false;
				await notificacionNormaSuscritaDao.Actualizar(notificacion, transaction);
			}
		}

		public async Task Eliminar(NotificacionNormaSuscrita notificacionNormaSuscrita, NpgsqlTransaction? transaction = null) {
			if (notificacionNormaSuscrita.Vigencia) {
				notificacionNormaSuscrita.FechaEliminacion = DateTime.UtcNow;
				notificacionNormaSuscrita.Vigencia = false;

				await notificacionNormaSuscritaDao.Actualizar(notificacionNormaSuscrita, transaction);
				await historialNormaSuscritaBcp.EliminarHistorialNotificacionesPorNormaSuscritaYAntelacion(notificacionNormaSuscrita.IdNormaSuscrita, notificacionNormaSuscrita.IdTipoUnidadTiempoAntelacion, notificacionNormaSuscrita.CantAntelacion, transaction);
			}
		}

		public async Task Crear(long idNormaSuscrita, long idTipoUnidadTiempoAntelacion, int cantAntelacion, NpgsqlTransaction? transaction = null) {
			NotificacionNormaSuscrita nuevo = new() {
				Id = 0,
				IdNormaSuscrita = idNormaSuscrita,
				IdTipoUnidadTiempoAntelacion = idTipoUnidadTiempoAntelacion,
				CantAntelacion = cantAntelacion,
				FechaCreacion = DateTime.UtcNow,
				FechaEliminacion = null,
				Vigencia = true
			};

			nuevo.Id = await notificacionNormaSuscritaDao.Insertar(nuevo, transaction);



			await historialNormaSuscritaBcp.CrearHistorialNotificacionesPorNormaSuscritaYAntelacion(nuevo.IdNormaSuscrita, nuevo.IdTipoUnidadTiempoAntelacion, nuevo.CantAntelacion, transaction);
		}
	}
}
