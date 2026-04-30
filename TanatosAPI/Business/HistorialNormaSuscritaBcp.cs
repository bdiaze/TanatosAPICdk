using Microsoft.AspNetCore.SignalR;
using Npgsql;
using System.Formats.Asn1;
using TanatosAPI.Entities.Models;
using TanatosAPI.Repositories;
using TimeZoneConverter;

namespace TanatosAPI.Business {
	public class HistorialNormaSuscritaBcp(HistorialNotificacionBcp historialNotificacionBcp, DocumentoAdjuntoBcp documentoAdjuntoBcp, NormaSuscritaDao normaSuscritaDao, HistorialNormaSuscritaDao historialNormaSuscritaDao, HistorialNotificacionDao historialNotificacionDao, NotificacionNormaSuscritaDao notificacionNormaSuscritaDao, DestinatarioNotificacionDao destinatarioNotificacionDao, TemplateNormaDao templateNormaDao, TemplateNormaNotificacionDao templateNormaNotificacionDao, TipoUnidadTiempoDao tipoUnidadTiempoDao, TipoPeriodicidadDao tipoPeriodicidadDao) {
		public async Task ActualizarHistorialNotificacionPorNormaSuscrita(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null) {
			List<DestinatarioNotificacion> destinatarios = [.. (await destinatarioNotificacionDao.ObtenerPorSub(normaSuscrita.Sub, normaSuscrita.IdNegocio, true, transaction)).Where(d => d.Validado)];
			List<NotificacionNormaSuscrita> notificacionNormaSuscritas = await notificacionNormaSuscritaDao.ObtenerPorNormaSuscrita(normaSuscrita.Id, true, transaction);
			List<TemplateNormaNotificacion> templateNormaNotificaciones = (normaSuscrita.IdTemplate != null && normaSuscrita.IdNorma != null && notificacionNormaSuscritas.Count == 0)
				? await templateNormaNotificacionDao.ObtenerPorTemplateNorma(normaSuscrita.IdTemplate.Value, normaSuscrita.IdNorma, transaction)
				: [];

			HashSet<(long? IdTipoUnidadTiempoAntelacion, int? CantAntelacion)> notificacionesAntelacion = [];
			notificacionesAntelacion.Add((null, null));
			foreach (NotificacionNormaSuscrita notificacionNormaSuscrita in notificacionNormaSuscritas) {
				notificacionesAntelacion.Add((notificacionNormaSuscrita.IdTipoUnidadTiempoAntelacion, notificacionNormaSuscrita.CantAntelacion));
			}
			foreach (TemplateNormaNotificacion templateNormaNotificacion in templateNormaNotificaciones) {
				notificacionesAntelacion.Add((templateNormaNotificacion.IdTipoUnidadTiempoAntelacion, templateNormaNotificacion.CantAntelacion));
			}

			HashSet<(long idDestinatarioNotificacion, long? IdTipoUnidadTiempoAntelacion, int? CantAntelacion)> historialNotificaciones = [];
			foreach (DestinatarioNotificacion destinatarioNotificacion in destinatarios) {
				foreach ((long? IdTipoUnidadTiempoAntelacion, int? CantAntelacion) antelacion in notificacionesAntelacion) {
					historialNotificaciones.Add((destinatarioNotificacion.Id, antelacion.IdTipoUnidadTiempoAntelacion, antelacion.CantAntelacion));
				}
			}

			List<HistorialNormaSuscrita> historialNormasSuscritasVigentes = [.. (await historialNormaSuscritaDao.ObtenerPorNormaSuscritaYFechaCompletitud(normaSuscrita.Id, null, true, transaction)).Where(hns => hns.FechaVencimiento > DateTime.UtcNow)];
			foreach (HistorialNormaSuscrita historialNormaSuscrita in historialNormasSuscritasVigentes) {
				await historialNotificacionBcp.ActualizarPorHistorialNormaSuscrita(historialNormaSuscrita, historialNotificaciones, transaction);
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
							DateTime fechaProgramacion;
							if (unidadTiempo.CantDias != null) {
								long diasPrevios = notificacionNormaSuscrita.CantAntelacion * unidadTiempo.CantDias.Value;
								fechaProgramacion = historialNormaSuscrita.FechaVencimiento.AddDays(-1 * diasPrevios);
							} else if (unidadTiempo.CantHoras != null) {
								long horasPrevias = notificacionNormaSuscrita.CantAntelacion * unidadTiempo.CantHoras.Value;
								fechaProgramacion = historialNormaSuscrita.FechaVencimiento.AddHours(-1 * horasPrevias);
							} else if (unidadTiempo.CantMinutos != null) {
								long minutosPrevios = notificacionNormaSuscrita.CantAntelacion * unidadTiempo.CantMinutos.Value;
								fechaProgramacion = historialNormaSuscrita.FechaVencimiento.AddMinutes(-1 * minutosPrevios);
							} else {
								long segundosPrevios = notificacionNormaSuscrita.CantAntelacion * unidadTiempo.CantSegundos;
								fechaProgramacion = historialNormaSuscrita.FechaVencimiento.AddSeconds(-1 * segundosPrevios);
							}

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
							DateTime fechaProgramacion;
							if (unidadTiempo.CantDias != null) {
								long diasPrevios = templateNormaNotificacion.CantAntelacion * unidadTiempo.CantDias.Value;
								fechaProgramacion = historialNormaSuscrita.FechaVencimiento.AddDays(-1 * diasPrevios);
							} else if (unidadTiempo.CantHoras != null) {
								long horasPrevias = templateNormaNotificacion.CantAntelacion * unidadTiempo.CantHoras.Value;
								fechaProgramacion = historialNormaSuscrita.FechaVencimiento.AddHours(-1 * horasPrevias);
							} else if (unidadTiempo.CantMinutos != null) {
								long minutosPrevios = templateNormaNotificacion.CantAntelacion * unidadTiempo.CantMinutos.Value;
								fechaProgramacion = historialNormaSuscrita.FechaVencimiento.AddMinutes(-1 * minutosPrevios);
							} else {
								long segundosPrevios = templateNormaNotificacion.CantAntelacion * unidadTiempo.CantSegundos;
								fechaProgramacion = historialNormaSuscrita.FechaVencimiento.AddSeconds(-1 * segundosPrevios);
							}

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
				await documentoAdjuntoBcp.EliminarPorHistorialNormaSuscrita(historial.Id, transaction);
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

					DateTime fechaProgramacion;
					if (tipoUnidadTiempo.CantDias != null) {
						long diasPrevios = cantAntelacion * tipoUnidadTiempo.CantDias.Value;
						fechaProgramacion = historial.FechaVencimiento.AddDays(-1 * diasPrevios);
					} else if (tipoUnidadTiempo.CantHoras != null) {
						long horasPrevias = cantAntelacion * tipoUnidadTiempo.CantHoras.Value;
						fechaProgramacion = historial.FechaVencimiento.AddHours(-1 * horasPrevias);
					} else if (tipoUnidadTiempo.CantMinutos != null) {
						long minutosPrevios = cantAntelacion * tipoUnidadTiempo.CantMinutos.Value;
						fechaProgramacion = historial.FechaVencimiento.AddMinutes(-1 * minutosPrevios);
					} else {
						long segundosPrevios = cantAntelacion * tipoUnidadTiempo.CantSegundos;
						fechaProgramacion = historial.FechaVencimiento.AddSeconds(-1 * segundosPrevios);
					}

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

		public async Task CompletarHistorialNormaSuscrita(HistorialNormaSuscrita historialNormaSuscrita, NpgsqlTransaction? transaction = null) {
			if (historialNormaSuscrita.FechaCompletitud == null) {
				historialNormaSuscrita.FechaCompletitud = DateTime.UtcNow;
				await historialNormaSuscritaDao.Actualizar(historialNormaSuscrita, transaction);
				await historialNotificacionBcp.EliminarPorHistorialNormaSuscrita(historialNormaSuscrita.Id, transaction);

				await ProgramarSiguienteVencimiento(historialNormaSuscrita, transaction);
			}
		}

		public async Task ProgramarSiguienteVencimiento(HistorialNormaSuscrita historialNormaSuscrita, NpgsqlTransaction? transaction = null) {
			// Solo se programa el siguiente vencimiento si no existe otro vencimiento futuro...
			List<HistorialNormaSuscrita> historialesFuturos = [.. (await historialNormaSuscritaDao.ObtenerPorNormaSuscrita(historialNormaSuscrita.IdNormaSuscrita, null, true, transaction)).Where(hns => hns.FechaVencimiento > historialNormaSuscrita.FechaVencimiento)];
			if (historialesFuturos.Count > 0) {
				return;
			}

			// Se obtiene norma suscrita y/o template...
			NormaSuscrita normaSuscrita = await normaSuscritaDao.ObtenerPorId(historialNormaSuscrita.IdNormaSuscrita, transaction) ?? throw new Exception("ID norma suscrita inválida");
			TemplateNorma? templateNorma = null;
			if (normaSuscrita.IdTemplate != null && normaSuscrita.IdNorma != null && normaSuscrita.IdTipoPeriodicidad == null) {
				templateNorma = (await templateNormaDao.ObtenerPorTemplate(normaSuscrita.IdTemplate.Value, transaction)).FirstOrDefault(n => n.IdNorma == normaSuscrita.IdNorma);
			}

			TipoPeriodicidad tipoPeriodicidad = await tipoPeriodicidadDao.ObtenerPorId((normaSuscrita.IdTipoPeriodicidad ?? templateNorma?.IdTipoPeriodicidad!).Value, transaction) ?? throw new Exception("Tipo periodicidad inválido");
			if (!string.IsNullOrWhiteSpace(tipoPeriodicidad.Cron)) {
				// Nos aseguramos de que la fecha esté en UTC...
				DateTime vencimientoActual = DateTime.SpecifyKind(historialNormaSuscrita.FechaVencimiento, DateTimeKind.Utc);

				// Se transforma la fecha de vencimiento actual a zona horaria de Chile...
				string timezone = "America/Santiago";
				if (OperatingSystem.IsWindows()) {
					timezone = TZConvert.IanaToWindows(timezone);
				}
				TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);
				DateTime proximoVencimiento = TimeZoneInfo.ConvertTimeFromUtc(vencimientoActual, timeZoneInfo);

				// Se añaden los deltas de la periodicidad...
				if (tipoPeriodicidad.DeltaDias != null) {
					proximoVencimiento = proximoVencimiento.AddDays(tipoPeriodicidad.DeltaDias.Value);
				}
				if (tipoPeriodicidad.DeltaMeses != null) {
					proximoVencimiento = proximoVencimiento.AddMonths(tipoPeriodicidad.DeltaMeses.Value);
				}
				if (tipoPeriodicidad.DeltaAnnos != null) {
					proximoVencimiento = proximoVencimiento.AddYears(tipoPeriodicidad.DeltaAnnos.Value);
				}

				// Se convierte próximo vencimiento calculado a UTC...
				proximoVencimiento = TimeZoneInfo.ConvertTimeToUtc(proximoVencimiento, timeZoneInfo);

				if (vencimientoActual != proximoVencimiento) {
					// Se crea el próximo vencimiento...
					HistorialNormaSuscrita nuevoHistorialNormaSuscrita = new() {
						Id = 0,
						IdNormaSuscrita = historialNormaSuscrita.IdNormaSuscrita,
						FechaVencimiento = proximoVencimiento,
						FechaCreacion = DateTime.UtcNow,
						Vigencia = true
					};

					await Crear(nuevoHistorialNormaSuscrita, transaction);
				}
			}
		}
	}
}
