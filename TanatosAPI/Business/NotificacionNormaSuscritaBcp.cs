using Amazon.Lambda.Core;
using Npgsql;
using System.Diagnostics;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class NotificacionNormaSuscritaBcp(NotificacionNormaSuscritaDao notificacionNormaSuscritaDao) {
		public async Task ActualizarPorNormaSuscrita(NormaSuscrita normaSuscrita, List<NotificacionNormaSuscrita> notificacionesNormaSuscrita, NpgsqlTransaction? transaction = null) {
			List<NotificacionNormaSuscrita> notificacionesExistentes = await notificacionNormaSuscritaDao.ObtenerPorNormaSuscrita(normaSuscrita.Id, true, transaction);

			// Se eliminan las notificaciones existentes que no se incluyen en la entrada...
			foreach (NotificacionNormaSuscrita notificacionExistente in notificacionesExistentes) {
				if (!notificacionesNormaSuscrita.Any(n => n.IdTipoUnidadTiempoAntelacion == notificacionExistente.IdTipoUnidadTiempoAntelacion && n.CantAntelacion == notificacionExistente.CantAntelacion)) {
					notificacionExistente.FechaEliminacion = DateTime.UtcNow;
					notificacionExistente.Vigencia = false;
					await notificacionNormaSuscritaDao.Actualizar(notificacionExistente, transaction);
				}
			}

			// Se agregan las nuevas notificaciones...
			foreach (NotificacionNormaSuscrita notificacionNueva in notificacionesNormaSuscrita) {
				if (!notificacionesExistentes.Any(ne => ne.IdTipoUnidadTiempoAntelacion == notificacionNueva.IdTipoUnidadTiempoAntelacion && ne.CantAntelacion == notificacionNueva.CantAntelacion)) {
					notificacionNueva.IdNormaSuscrita = normaSuscrita.Id;
					notificacionNueva.FechaCreacion = DateTime.UtcNow;
					notificacionNueva.FechaEliminacion = null;
					notificacionNueva.Vigencia = true;

					notificacionNueva.Id = await notificacionNormaSuscritaDao.Insertar(notificacionNueva, transaction);
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
	}
}
