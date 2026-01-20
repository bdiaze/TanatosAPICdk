using Microsoft.AspNetCore.SignalR;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class HistorialNormaSuscritaBcp(HistorialNotificacionBcp historialNotificacionBcp, NormaSuscritaDao normaSuscritaDao, HistorialNormaSuscritaDao historialNormaSuscritaDao, HistorialNotificacionDao historialNotificacionDao, NotificacionNormaSuscritaDao notificacionNormaSuscritaDao, DestinatarioNotificacionDao destinatarioNotificacionDao, TemplateNormaNotificacionDao templateNormaNotificacionDao, TipoUnidadTiempoDao tipoUnidadTiempoDao) {
		public async Task Crear(HistorialNormaSuscrita historialNormaSuscrita, NpgsqlTransaction? transaction = null) {
			historialNormaSuscrita.Id = await historialNormaSuscritaDao.Insertar(historialNormaSuscrita, transaction);

			// Se obtienen los destinatarios para crear los historiales de notificación...
			NormaSuscrita normaSuscrita = await normaSuscritaDao.ObtenerPorId(historialNormaSuscrita.IdNormaSuscrita, transaction) ?? throw new Exception("Norma suscrita no encontrada");
			List<DestinatarioNotificacion> destinatariosNotificaciones = await destinatarioNotificacionDao.ObtenerPorSub(normaSuscrita.Sub, normaSuscrita.IdNegocio, true, transaction);

			List<TipoUnidadTiempo> tiposUnidadesTiempo = [];

			// Se obtienen las notificaciones asociadas a la norma suscrita, o al template de norma...
			List<NotificacionNormaSuscrita> notificacionesNormaSuscrita = [];
			List<TemplateNormaNotificacion> templateNormaNotificaciones = [];
			if (destinatariosNotificaciones.Any(dn => dn.Validado)) {
				notificacionesNormaSuscrita = await notificacionNormaSuscritaDao.ObtenerPorNormaSuscrita(normaSuscrita.Id, true, transaction);

				if (notificacionesNormaSuscrita.Count == 0 && normaSuscrita.IdTemplate != null && normaSuscrita.IdNorma != null) {
					templateNormaNotificaciones = await templateNormaNotificacionDao.ObtenerPorTemplateNorma(normaSuscrita.IdTemplate.Value, normaSuscrita.IdNorma, transaction);
				}

				if (notificacionesNormaSuscrita.Count > 0 || templateNormaNotificaciones.Count > 0) {
					tiposUnidadesTiempo = await tipoUnidadTiempoDao.ObtenerPorVigencia(true, transaction);
				}
			}

			// Se crean los historiales de notificación...
			foreach (DestinatarioNotificacion destinatarioNotificacion in destinatariosNotificaciones.Where(dn => dn.Validado)) {
				await historialNotificacionDao.Insertar(new HistorialNotificacion {
					Id = 0,
					IdHistorialNormaSuscrita = historialNormaSuscrita.Id,
					IdDestinatarioNotificacion = destinatarioNotificacion.Id,
					FechaProgramacion = historialNormaSuscrita.FechaVencimiento
				}, transaction);

				if (notificacionesNormaSuscrita.Count > 0) {
					foreach (NotificacionNormaSuscrita notificacionNormaSuscrita in notificacionesNormaSuscrita) {
						TipoUnidadTiempo? unidadTiempo = tiposUnidadesTiempo.FirstOrDefault(tut => tut.Id == notificacionNormaSuscrita.IdTipoUnidadTiempoAntelacion);

						if (unidadTiempo != null) {
							long segundosPrevios = notificacionNormaSuscrita.CantAntelacion * unidadTiempo.CantSegundos;
							DateTime fechaProgramacion = historialNormaSuscrita.FechaVencimiento.AddSeconds(-1 * segundosPrevios);

							await historialNotificacionDao.Insertar(new HistorialNotificacion {
								Id = 0,
								IdHistorialNormaSuscrita = historialNormaSuscrita.Id,
								IdDestinatarioNotificacion = destinatarioNotificacion.Id,
								FechaProgramacion = fechaProgramacion
							}, transaction);
						}
					}
				} else if (templateNormaNotificaciones.Count > 0) {
					foreach (TemplateNormaNotificacion templateNormaNotificacion in templateNormaNotificaciones) {
						TipoUnidadTiempo? unidadTiempo = tiposUnidadesTiempo.FirstOrDefault(tut => tut.Id == templateNormaNotificacion.IdTipoUnidadTiempoAntelacion);

						if (unidadTiempo != null) {
							long segundosPrevios = templateNormaNotificacion.CantAntelacion * unidadTiempo.CantSegundos;
							DateTime fechaProgramacion = historialNormaSuscrita.FechaVencimiento.AddSeconds(-1 * segundosPrevios);

							await historialNotificacionDao.Insertar(new HistorialNotificacion {
								Id = 0,
								IdHistorialNormaSuscrita = historialNormaSuscrita.Id,
								IdDestinatarioNotificacion = destinatarioNotificacion.Id,
								FechaProgramacion = fechaProgramacion
							}, transaction);
						}
					}
				}
			}
		}

		public async Task EliminarPorNormaSuscrita(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null) {
			List<HistorialNormaSuscrita> historialesVigentes = await historialNormaSuscritaDao.ObtenerPorNormaSuscritaYFechaCompletitud(normaSuscrita.Id, null, true, transaction);
			foreach (HistorialNormaSuscrita historial in historialesVigentes) {
				historial.FechaEliminacion = DateTime.UtcNow;
				historial.Vigencia = false;
				await historialNormaSuscritaDao.Actualizar(historial, transaction);

				await historialNotificacionBcp.EliminarPorHistorialNormaSuscrita(historial, transaction);
			}
		}
	}
}
