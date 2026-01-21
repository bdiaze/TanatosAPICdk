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
			List<DestinatarioNotificacion> destinatariosNotificaciones = [.. (await destinatarioNotificacionDao.ObtenerPorSub(normaSuscrita.Sub, normaSuscrita.IdNegocio, true, transaction)).Where(dn => dn.Validado)];

			List<TipoUnidadTiempo> tiposUnidadesTiempo = [];

			// Se obtienen las notificaciones asociadas a la norma suscrita, o al template de norma...
			List<NotificacionNormaSuscrita> notificacionesNormaSuscrita = [];
			List<TemplateNormaNotificacion> templateNormaNotificaciones = [];
			if (destinatariosNotificaciones.Count > 0) {
				notificacionesNormaSuscrita = await notificacionNormaSuscritaDao.ObtenerPorNormaSuscrita(normaSuscrita.Id, true, transaction);

				if (notificacionesNormaSuscrita.Count == 0 && normaSuscrita.IdTemplate != null && normaSuscrita.IdNorma != null) {
					templateNormaNotificaciones = await templateNormaNotificacionDao.ObtenerPorTemplateNorma(normaSuscrita.IdTemplate.Value, normaSuscrita.IdNorma, transaction);
				}

				if (notificacionesNormaSuscrita.Count > 0 || templateNormaNotificaciones.Count > 0) {
					tiposUnidadesTiempo = await tipoUnidadTiempoDao.ObtenerPorVigencia(true, transaction);
				}
			}

			// Se crean los historiales de notificación...
			foreach (DestinatarioNotificacion destinatarioNotificacion in destinatariosNotificaciones) {
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
								IdTipoUnidadTiempoAntelacion = notificacionNormaSuscrita.IdTipoUnidadTiempoAntelacion,
								CantAntelacion = notificacionNormaSuscrita.CantAntelacion,
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
								IdTipoUnidadTiempoAntelacion = templateNormaNotificacion.IdTipoUnidadTiempoAntelacion,
								CantAntelacion = templateNormaNotificacion.CantAntelacion,
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

		public async Task EliminarHistorialNotificacionesPorNormaSuscritaYAntelacion(long idNormaSuscrita, long idTipoUnidadTiempoAntelacion, int cantAntelacion, NpgsqlTransaction? transaction = null) {
			List<HistorialNormaSuscrita> historialesVigentes = await historialNormaSuscritaDao.ObtenerPorNormaSuscritaYFechaCompletitud(idNormaSuscrita, null, true, transaction);
			foreach (HistorialNormaSuscrita historial in historialesVigentes) {
				await historialNotificacionBcp.EliminarPorHistorialNormaSuscritaYAntelacion(historial.Id, idTipoUnidadTiempoAntelacion, cantAntelacion, transaction);
			}
		}

		public async Task CrearHistorialNotificacionesPorNormaSuscritaYAntelacion(long idNormaSuscrita, long idTipoUnidadTiempoAntelacion, int cantAntelacion, NpgsqlTransaction? transaction = null) {
			List<HistorialNormaSuscrita> historialesVigentes = await historialNormaSuscritaDao.ObtenerPorNormaSuscritaYFechaCompletitud(idNormaSuscrita, null, true, transaction);

			// Se obtienen los destinatarios para crear los historiales de notificación...
			NormaSuscrita normaSuscrita = await normaSuscritaDao.ObtenerPorId(idNormaSuscrita, transaction) ?? throw new Exception("Norma suscrita no encontrada");
			List<DestinatarioNotificacion> destinatariosNotificaciones = [.. (await destinatarioNotificacionDao.ObtenerPorSub(normaSuscrita.Sub, normaSuscrita.IdNegocio, true, transaction)).Where(dn => dn.Validado)];

			TipoUnidadTiempo? tipoUnidadTiempo = await tipoUnidadTiempoDao.ObtenerPorId(idTipoUnidadTiempoAntelacion, transaction);
			if (tipoUnidadTiempo != null && tipoUnidadTiempo.Vigencia) {
				foreach (HistorialNormaSuscrita historial in historialesVigentes) {
					long segundosPrevios = cantAntelacion * tipoUnidadTiempo.CantSegundos;
					DateTime fechaProgramacion = historial.FechaVencimiento.AddSeconds(-1 * segundosPrevios);

					foreach (DestinatarioNotificacion destinatario in destinatariosNotificaciones) {
						await historialNotificacionBcp.CrearPorHistorialNormaSuscritaYAntelacion(historial.Id, destinatario.Id, idTipoUnidadTiempoAntelacion, cantAntelacion, fechaProgramacion, transaction);
					}
				}
			}
		}
	}
}
