using Amazon.Lambda.Core;
using Npgsql;
using System.Diagnostics;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class NotificacionNormaSuscritaBcp(IDateTimeProvider dateTimeProvider, INotificacionNormaSuscritaDao notificacionNormaSuscritaDao) : INotificacionNormaSuscritaBcp {
		public async Task<List<NotificacionNormaSuscrita>> ObtenerVigentesPorNormaSuscrita(long idNormaSuscrita, NpgsqlTransaction? transaction = null) {
			return await notificacionNormaSuscritaDao.ObtenerPorNormaSuscrita(idNormaSuscrita, true, transaction);
		}
		
		public async Task Eliminar(NotificacionNormaSuscrita notificacionNormaSuscrita, NpgsqlTransaction? transaction = null) {
			if (notificacionNormaSuscrita.Vigencia) {
				notificacionNormaSuscrita.FechaEliminacion = dateTimeProvider.UtcNow;
				notificacionNormaSuscrita.Vigencia = false;

				await notificacionNormaSuscritaDao.Actualizar(notificacionNormaSuscrita, transaction);
			}
		}

		public async Task EliminarPorNormaSuscrita(long idNormaSuscrita, NpgsqlTransaction? transaction = null) {
			List<NotificacionNormaSuscrita> notificacionesVigentes = await notificacionNormaSuscritaDao.ObtenerPorNormaSuscrita(idNormaSuscrita, true);
			foreach (NotificacionNormaSuscrita notificacion in notificacionesVigentes) {
				await Eliminar(notificacion, transaction);
			}
		}

		public async Task<NotificacionNormaSuscrita> Insertar(long idNormaSuscrita, long idTipoUnidadTiempoAntelacion, int cantAntelacion, NpgsqlTransaction? transaction = null) {
			NotificacionNormaSuscrita nuevo = new() {
				Id = 0,
				IdNormaSuscrita = idNormaSuscrita,
				IdTipoUnidadTiempoAntelacion = idTipoUnidadTiempoAntelacion,
				CantAntelacion = cantAntelacion,
				FechaCreacion = dateTimeProvider.UtcNow,
				FechaEliminacion = null,
				Vigencia = true
			};
			nuevo.Id = await notificacionNormaSuscritaDao.Insertar(nuevo, transaction);
			return nuevo;
		}

		public async Task ActualizarPorNormaSuscrita(long idNormaSuscrita, HashSet<(long IdTipoUnidadTiempoAntelacion, int CantAntelacion)> notificacionesNormaSuscrita, NpgsqlTransaction? transaction = null) {
			List<NotificacionNormaSuscrita> notificacionesExistentes = await notificacionNormaSuscritaDao.ObtenerPorNormaSuscrita(idNormaSuscrita, true, transaction);
			HashSet<(long IdTipoUnidadTiempoAntelacion, int CantAntelacion)> existentes = [.. notificacionesExistentes.Select(n => (n.IdTipoUnidadTiempoAntelacion, n.CantAntelacion))];

			// Se eliminan las notificaciones normas existentes que no se incluyen en la entrada...
			foreach (NotificacionNormaSuscrita notificacionExistente in notificacionesExistentes) {
				if (!notificacionesNormaSuscrita.Contains((notificacionExistente.IdTipoUnidadTiempoAntelacion, notificacionExistente.CantAntelacion))) {
					await Eliminar(notificacionExistente, transaction);
				}
			}

			// Se agregan las nuevas notificaciones normas...
			foreach ((long IdTipoUnidadTiempoAntelacion, int CantAntelacion) notificacionNueva in notificacionesNormaSuscrita) {
				if (!existentes.Contains(notificacionNueva)) {
					await Insertar(idNormaSuscrita, notificacionNueva.IdTipoUnidadTiempoAntelacion, notificacionNueva.CantAntelacion, transaction);
				}
			}
		}
	}
}
