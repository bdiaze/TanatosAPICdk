using Amazon.Lambda.Core;
using Npgsql;
using System.Diagnostics;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class NotificacionNormaSuscritaBcp(NotificacionNormaSuscritaDao notificacionNormaSuscritaDao, DestinatarioNotificacionDao destinatarioNotificacionDao, TemplateNormaNotificacionDao templateNormaNotificacionDao, HistorialNormaSuscritaBcp historialNormaSuscritaBcp, HistorialNotificacionBcp historialNotificacionBcp) {
		public async Task ActualizarPorNormaSuscrita(NormaSuscrita normaSuscrita, HashSet<(long IdTipoUnidadTiempoAntelacion, int CantAntelacion)> notificacionesNormaSuscrita, NpgsqlTransaction? transaction = null) {
			List<NotificacionNormaSuscrita> notificacionesExistentes = await notificacionNormaSuscritaDao.ObtenerPorNormaSuscrita(normaSuscrita.Id, true, transaction);
			
			// Se eliminan las notificaciones normas existentes que no se incluyen en la entrada...
			foreach (NotificacionNormaSuscrita notificacionExistente in notificacionesExistentes) {
				if (!notificacionesNormaSuscrita.Any(n => n.IdTipoUnidadTiempoAntelacion == notificacionExistente.IdTipoUnidadTiempoAntelacion && n.CantAntelacion == notificacionExistente.CantAntelacion)) {
					notificacionExistente.FechaEliminacion = DateTime.UtcNow;
					notificacionExistente.Vigencia = false;

					await notificacionNormaSuscritaDao.Actualizar(notificacionExistente, transaction);
				}
			}

			// Se agregan las nuevas notificaciones normas...
			foreach ((long IdTipoUnidadTiempoAntelacion, int CantAntelacion) notificacionNueva in notificacionesNormaSuscrita) {
				if (!notificacionesExistentes.Any(ne => ne.IdTipoUnidadTiempoAntelacion == notificacionNueva.IdTipoUnidadTiempoAntelacion && ne.CantAntelacion == notificacionNueva.CantAntelacion)) {
					await notificacionNormaSuscritaDao.Insertar(new() {
						Id = 0,
						IdNormaSuscrita = normaSuscrita.Id,
						IdTipoUnidadTiempoAntelacion = notificacionNueva.IdTipoUnidadTiempoAntelacion,
						CantAntelacion = notificacionNueva.CantAntelacion,
						FechaCreacion = DateTime.UtcNow,
						FechaEliminacion = null,
						Vigencia = true
					}, transaction);
				}
			}

			List<TemplateNormaNotificacion> notificacionesTemplate = [];
			// Si el cliente no define sus propias notificaciones, se obtienen la información del template...
			if (notificacionesNormaSuscrita.Count == 0 && normaSuscrita.IdTemplate != null && normaSuscrita.IdNorma != null) {
				notificacionesTemplate = await templateNormaNotificacionDao.ObtenerPorTemplateNorma(normaSuscrita.IdTemplate.Value, normaSuscrita.IdNorma, transaction);
			}

			// Una vez se tienen actualizadas las notificaciones norma, se procede a actualizar los historiales de notificación...
			List<DestinatarioNotificacion> destinatarios = [.. (await destinatarioNotificacionDao.ObtenerPorSub(normaSuscrita.Sub, normaSuscrita.IdNegocio, true, transaction)).Where(d => d.Validado)];
			HashSet<(long idDestinatarioNotificacion, long? IdTipoUnidadTiempoAntelacion, int? CantAntelacion)> historialNotificaciones = [];

			// Se añaden las notificaciones que son previas al vencimiento...
			foreach (DestinatarioNotificacion destinatario in destinatarios) {
				// Se añaden las notificaciones que se ejecutan en tiempo de vencimiento...
				historialNotificaciones.Add((destinatario.Id, null, null));

				foreach ((long IdTipoUnidadTiempoAntelacion, int CantAntelacion) notificacionNorma in notificacionesNormaSuscrita) {
					historialNotificaciones.Add((destinatario.Id, notificacionNorma.IdTipoUnidadTiempoAntelacion, notificacionNorma.CantAntelacion));
				}

				foreach(TemplateNormaNotificacion templateNotificacion in notificacionesTemplate) {
					historialNotificaciones.Add((destinatario.Id, templateNotificacion.IdTipoUnidadTiempoAntelacion, templateNotificacion.CantAntelacion));
				}
			}

			await historialNormaSuscritaBcp.ActualizarHistorialNotificacionPorNormaSuscrita(normaSuscrita, historialNotificaciones, transaction);
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
