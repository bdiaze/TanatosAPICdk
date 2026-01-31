using Microsoft.AspNetCore.SignalR;
using Npgsql;
using System.Formats.Asn1;
using TanatosAPI.Entities.Models;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class HistorialNormaSuscritaBcp(HistorialNotificacionBcp historialNotificacionBcp, NormaSuscritaDao normaSuscritaDao, HistorialNormaSuscritaDao historialNormaSuscritaDao, HistorialNotificacionDao historialNotificacionDao, NotificacionNormaSuscritaDao notificacionNormaSuscritaDao, DestinatarioNotificacionDao destinatarioNotificacionDao, TemplateNormaNotificacionDao templateNormaNotificacionDao, TipoUnidadTiempoDao tipoUnidadTiempoDao) {
		public async Task ActualizarHistorialNotificacionPorNormaSuscrita(NormaSuscrita normaSuscrita, HashSet<(long idDestinatarioNotificacion, long? IdTipoUnidadTiempoAntelacion, int? CantAntelacion)> historialesNotificaciones, NpgsqlTransaction? transaction = null) {
			List<HistorialNormaSuscrita> historialNormasSuscritasVigentes =  await historialNormaSuscritaDao.ObtenerPorNormaSuscritaYFechaCompletitud(normaSuscrita.Id, null, true, transaction);

			foreach (HistorialNormaSuscrita historialNormaSuscrita in historialNormasSuscritasVigentes) {
				await historialNotificacionBcp.ActualizarPorHistorialNormaSuscrita(historialNormaSuscrita, historialesNotificaciones, transaction);
			}
		}


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
				await historialNotificacionBcp.Crear(historialNormaSuscrita.Id, destinatarioNotificacion.Id, null, null, historialNormaSuscrita.FechaVencimiento, transaction);

				if (notificacionesNormaSuscrita.Count > 0) {
					foreach (NotificacionNormaSuscrita notificacionNormaSuscrita in notificacionesNormaSuscrita) {
						TipoUnidadTiempo? unidadTiempo = tiposUnidadesTiempo.FirstOrDefault(tut => tut.Id == notificacionNormaSuscrita.IdTipoUnidadTiempoAntelacion);

						if (unidadTiempo != null) {
							long segundosPrevios = notificacionNormaSuscrita.CantAntelacion * unidadTiempo.CantSegundos;
							DateTime fechaProgramacion = historialNormaSuscrita.FechaVencimiento.AddSeconds(-1 * segundosPrevios);

							await historialNotificacionBcp.Crear(
								historialNormaSuscrita.Id, 
								destinatarioNotificacion.Id, 
								notificacionNormaSuscrita.IdTipoUnidadTiempoAntelacion, 
								notificacionNormaSuscrita.CantAntelacion, 
								fechaProgramacion, 
								transaction
							);
						}
					}
				} else if (templateNormaNotificaciones.Count > 0) {
					foreach (TemplateNormaNotificacion templateNormaNotificacion in templateNormaNotificaciones) {
						TipoUnidadTiempo? unidadTiempo = tiposUnidadesTiempo.FirstOrDefault(tut => tut.Id == templateNormaNotificacion.IdTipoUnidadTiempoAntelacion);

						if (unidadTiempo != null) {
							long segundosPrevios = templateNormaNotificacion.CantAntelacion * unidadTiempo.CantSegundos;
							DateTime fechaProgramacion = historialNormaSuscrita.FechaVencimiento.AddSeconds(-1 * segundosPrevios);

							await historialNotificacionBcp.Crear(
								historialNormaSuscrita.Id,
								destinatarioNotificacion.Id,
								templateNormaNotificacion.IdTipoUnidadTiempoAntelacion,
								templateNormaNotificacion.CantAntelacion,
								fechaProgramacion,
								transaction
							);
						}
					}
				}
			}
		}

		public async Task EliminarPorNormaSuscrita(NormaSuscrita normaSuscrita, bool ignorarVencidos = false, NpgsqlTransaction? transaction = null) {
			List<HistorialNormaSuscrita> historialesVigentes = await historialNormaSuscritaDao.ObtenerPorNormaSuscritaYFechaCompletitud(normaSuscrita.Id, null, true, transaction);

			if (ignorarVencidos) {
				historialesVigentes = [.. historialesVigentes.Where(h => h.FechaVencimiento > DateTime.UtcNow)];
			}

			foreach (HistorialNormaSuscrita historial in historialesVigentes) {
				historial.FechaEliminacion = DateTime.UtcNow;
				historial.Vigencia = false;
				await historialNormaSuscritaDao.Actualizar(historial, transaction);

				await historialNotificacionBcp.EliminarPorHistorialNormaSuscrita(historial.Id, transaction);
			}
		}

		public async Task EliminarHistorialNotificacionesPorNormaSuscritaYAntelacion(long idNormaSuscrita, long idTipoUnidadTiempoAntelacion, int cantAntelacion, NpgsqlTransaction? transaction = null) {
			List<HistorialNormaSuscrita> historialesVigentes = await historialNormaSuscritaDao.ObtenerPorNormaSuscritaYFechaCompletitud(idNormaSuscrita, null, true, transaction);
			foreach (HistorialNormaSuscrita historial in historialesVigentes) {
				await historialNotificacionBcp.EliminarPorHistorialNormaSuscritaYAntelacion(historial.Id, idTipoUnidadTiempoAntelacion, cantAntelacion, transaction);
			}
		}

		public async Task CrearHistorialNotificacionesPorNormaSuscritaYAntelacion(long idNormaSuscrita, long idTipoUnidadTiempoAntelacion, int cantAntelacion, NpgsqlTransaction? transaction = null) {
			// Se obtienen los historiales de norma suscrita que estén vigentes y no completados...
			List<HistorialNormaSuscrita> historialesVigentes = await historialNormaSuscritaDao.ObtenerPorNormaSuscritaYFechaCompletitud(idNormaSuscrita, null, true, transaction);

			// Se obtienen los destinatarios para crear los historiales de notificación...
			NormaSuscrita normaSuscrita = await normaSuscritaDao.ObtenerPorId(idNormaSuscrita, transaction) ?? throw new Exception("Norma suscrita no encontrada");
			List<DestinatarioNotificacion> destinatariosNotificaciones = [.. (await destinatarioNotificacionDao.ObtenerPorSub(normaSuscrita.Sub, normaSuscrita.IdNegocio, true, transaction)).Where(dn => dn.Validado)];

			// Se obtiene el tipo de unidad de tiempo para calcular la fecha de programación...
			TipoUnidadTiempo? tipoUnidadTiempo = await tipoUnidadTiempoDao.ObtenerPorId(idTipoUnidadTiempoAntelacion, transaction);

			if (tipoUnidadTiempo != null && tipoUnidadTiempo.Vigencia) {
				// Por cada historial de norma suscrita, se crea el historial de notificación...
				foreach (HistorialNormaSuscrita historial in historialesVigentes) {
					// Se obtienen los historiales de notificación existentes, que estén vigentes y pertenezcan a la misma antelación...
					List<HistorialNotificacion> notificacionesVigentes = [.. (await historialNotificacionDao.ObtenerPorHistorial(historial.Id, null, true, transaction)).Where(n => n.IdTipoUnidadTiempoAntelacion == idTipoUnidadTiempoAntelacion && n.CantAntelacion == cantAntelacion)];

					long segundosPrevios = cantAntelacion * tipoUnidadTiempo.CantSegundos;
					DateTime fechaProgramacion = historial.FechaVencimiento.AddSeconds(-1 * segundosPrevios);

					// Solo se programan las notificaciones cuya fecha de programación sea futura...
					if (fechaProgramacion > DateTime.UtcNow) {
						foreach (DestinatarioNotificacion destinatario in destinatariosNotificaciones) {
							// Solo se crean las notificaciones que no existan aún...
							if (!notificacionesVigentes.Any(nv => nv.IdDestinatarioNotificacion == destinatario.Id)) {
								await historialNotificacionBcp.Crear(historial.Id, destinatario.Id, idTipoUnidadTiempoAntelacion, cantAntelacion, fechaProgramacion, transaction);
							}
						}
					}
				}
			}
		}
	}
}
