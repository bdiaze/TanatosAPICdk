using Npgsql;
using System;
using System.Diagnostics;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.Json;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Helpers;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class ProcesoNotificacionBcp(IHostEnvironment environment, VariableEntornoHelper variableEntornoHelper, HermesHelper hermesHelper, KairosHelper kairosHelper, NormaSuscritaDao normaSuscritaDao, TipoPeriodicidadDao tipoPeriodicidadDao, TipoUnidadTiempoDao tipoUnidadTiempoDao, HistorialNormaSuscritaDao historialNormaSuscritaDao, HistorialNotificacionDao historialNotificacionDao, NotificacionNormaSuscritaDao notificacionNormaSuscritaDao, TemplateNormaDao templateNormaDao, TemplateNormaNotificacionDao templateNormaNotificacionDao, DestinatarioNotificacionDao destinatarioNotificacionDao) {
		public async Task ActualizarProgramacionProcesosNormaSuscrita(long idNormaSuscrita, NpgsqlTransaction? transaction = null) {
			List<string> procesosProgramados = [];
			List<EntKairosIngresarProceso> procesosDesprogramados = [];
			try {
				NormaSuscrita normaSuscrita = await normaSuscritaDao.ObtenerPorId(idNormaSuscrita, transaction) ?? throw new Exception("Norma suscrita inválida");
				TemplateNorma? templateNorma = null;
				if (normaSuscrita.IdTipoPeriodicidad == null && normaSuscrita.IdTemplate != null && normaSuscrita.IdNorma != null) {
					templateNorma = (await templateNormaDao.ObtenerPorTemplate(normaSuscrita.IdTemplate.Value, transaction)).FirstOrDefault(n => n.IdNorma == normaSuscrita.IdNorma);
				}


				// Si la norma suscrita no está activada, se desprograman todas sus notificaciones...
				if (!normaSuscrita.Activado) {
					foreach (Dictionary<string, JsonElement> proceso in normaSuscrita.ProcesosNotificaciones ?? []) {
						if (proceso.TryGetValue("IdProceso", out JsonElement jsonIdProceso)) {
							string? idProceso = jsonIdProceso.ValueKind == JsonValueKind.String ? jsonIdProceso.GetString() : null;
							if (idProceso != null) {
								await kairosHelper.EliminarProceso(idProceso);

								if (proceso.TryGetValue("Nombre", out JsonElement jsonNombre) &&
									proceso.TryGetValue("Cron", out JsonElement jsonCron) &&
									proceso.TryGetValue("Parametros", out JsonElement jsonParametros) &&
									proceso.TryGetValue("ArnProceso", out JsonElement jsonArnProceso) &&
									proceso.TryGetValue("ArnRol", out JsonElement jsonArnRol)) {
									procesosDesprogramados.Add(new EntKairosIngresarProceso() {
										Nombre = jsonNombre.GetString()!,
										Cron = jsonCron.GetString()!,
										Parametros = jsonParametros.GetString()!,
										ArnProceso = jsonArnProceso.GetString()!,
										ArnRol = jsonArnRol.GetString()!,
										Habilitado = true
									});
								}
							}
						}
					}
					normaSuscrita.ProcesosNotificaciones = null;
					await normaSuscritaDao.Actualizar(normaSuscrita, transaction);

					// Si la norma suscrita está activada, se programan las notificaciones que no están programadas, y desprograman las que no son necesarias...
				} else if ((normaSuscrita.IdTipoPeriodicidad ?? templateNorma?.IdTipoPeriodicidad) != null) {
					TipoPeriodicidad tipoPeriodicidad = await tipoPeriodicidadDao.ObtenerPorId((normaSuscrita.IdTipoPeriodicidad ?? templateNorma?.IdTipoPeriodicidad!).Value, transaction) ?? throw new Exception("Tipo periodicidad inválido");

					if (!string.IsNullOrWhiteSpace(tipoPeriodicidad.Cron)) {
						// Se arma listado de las configuraciones de notificaciones previas...
						HashSet<(long? IdTipoUnidadTiempoAntelacion, int? CantAntelacion)> configNotifPrevias = [];
						configNotifPrevias.Add((null, null));

						List<NotificacionNormaSuscrita> notificacionesNormaSuscrita = await notificacionNormaSuscritaDao.ObtenerPorNormaSuscrita(idNormaSuscrita, true, transaction);
						foreach (NotificacionNormaSuscrita notificacionNormaSuscrita in notificacionesNormaSuscrita) {
							configNotifPrevias.Add((notificacionNormaSuscrita.IdTipoUnidadTiempoAntelacion, notificacionNormaSuscrita.CantAntelacion));
						}

						if (notificacionesNormaSuscrita.Count == 0 && normaSuscrita.IdTemplate != null && normaSuscrita.IdNorma != null) {
							List<TemplateNormaNotificacion> templateNormaNotificacions = await templateNormaNotificacionDao.ObtenerPorTemplateNorma(normaSuscrita.IdTemplate.Value, normaSuscrita.IdNorma, transaction);
							foreach (TemplateNormaNotificacion templateNormaNotificacion in templateNormaNotificacions) {
								configNotifPrevias.Add((templateNormaNotificacion.IdTipoUnidadTiempoAntelacion, templateNormaNotificacion.CantAntelacion));
							}
						}

						List<TipoUnidadTiempo> tiposUnidadTiempo = await tipoUnidadTiempoDao.ObtenerPorVigencia(true, transaction);

						HashSet<string> cronVencimiento = [];

						// Se arman los cron a programar según los próximos vencimientos...
						HashSet<string> crons = [];
						List<HistorialNormaSuscrita> historialNormaSuscritas = await historialNormaSuscritaDao.ObtenerPorNormaSuscritaYFechaCompletitud(idNormaSuscrita, null, true, transaction);
						foreach (HistorialNormaSuscrita historialNormaSuscrita in historialNormaSuscritas) {

							cronVencimiento.Add(CronHelper.GenerarCronDesdeFecha(historialNormaSuscrita.FechaVencimiento, tipoPeriodicidad.Cron));

							foreach ((long? IdTipoUnidadTiempoAntelacion, int? CantAntelacion) antelacion in configNotifPrevias) {
								DateTime fechaProgramacion = historialNormaSuscrita.FechaVencimiento;

								if (antelacion.IdTipoUnidadTiempoAntelacion != null && antelacion.CantAntelacion != null) {
									TipoUnidadTiempo? tipoUnidadTiempo = tiposUnidadTiempo.FirstOrDefault(ut => ut.Id == antelacion.IdTipoUnidadTiempoAntelacion);
									if (tipoUnidadTiempo != null) {
										long segundosPrevios = antelacion.CantAntelacion.Value * tipoUnidadTiempo.CantSegundos;
										fechaProgramacion = fechaProgramacion.AddSeconds(-1 * segundosPrevios);
									}
								}

								crons.Add(CronHelper.GenerarCronDesdeFecha(fechaProgramacion, tipoPeriodicidad.Cron));
							}
						}

						// Se obtienen los cron existentes...
						HashSet<string> cronsExistentes = [];
						foreach (Dictionary<string, JsonElement> proceso in normaSuscrita.ProcesosNotificaciones ?? []) {
							if (proceso.TryGetValue("Cron", out JsonElement jsonCronProceso)) {
								string? cron = jsonCronProceso.ValueKind == JsonValueKind.String ? jsonCronProceso.GetString() : null;
								if (cron != null) {
									cronsExistentes.Add(cron);
								}
							}
						}

						// Se eliminan los procesos programados que ya no aplican...
						List<Dictionary<string, JsonElement>> aEliminar = [];
						foreach (string cronExistente in cronsExistentes) {
							if (!crons.Any(c => c == cronExistente)) {
								aEliminar.AddRange(normaSuscrita.ProcesosNotificaciones!.Where(p =>
									p.TryGetValue("Cron", out JsonElement jsonCron) &&
									jsonCron.ValueKind == JsonValueKind.String &&
									jsonCron.GetString() == cronExistente)
								);
							}
						}

						foreach (Dictionary<string, JsonElement> eliminar in aEliminar) {
							if (eliminar.TryGetValue("IdProceso", out JsonElement jsonIdProceso)) {
								string? idProceso = jsonIdProceso.ValueKind == JsonValueKind.String ? jsonIdProceso.GetString() : null;
								if (idProceso != null) {
									await kairosHelper.EliminarProceso(idProceso);

									if (eliminar.TryGetValue("Nombre", out JsonElement jsonNombre) &&
										eliminar.TryGetValue("Cron", out JsonElement jsonCron) && 
										eliminar.TryGetValue("Parametros", out JsonElement jsonParametros) &&
										eliminar.TryGetValue("ArnProceso", out JsonElement jsonArnProceso) &&
										eliminar.TryGetValue("ArnRol", out JsonElement jsonArnRol)) {
										procesosDesprogramados.Add(new EntKairosIngresarProceso() {
											Nombre = jsonNombre.GetString()!,
											Cron = jsonCron.GetString()!,
											Parametros = jsonParametros.GetString()!,
											ArnProceso = jsonArnProceso.GetString()!,
											ArnRol = jsonArnRol.GetString()!,
											Habilitado = true
										});
									}
								}
							}
							normaSuscrita.ProcesosNotificaciones!.Remove(eliminar);
						}

						// Se crean los procesos programados que faltan...
						foreach (string cronNuevo in crons) {
							if (!cronsExistentes.Any(ce => ce == cronNuevo)) {
								string nombreProceso = $"{variableEntornoHelper.Obtener("APP_NAME")} - NormaSuscrita {idNormaSuscrita} - Cron {cronNuevo}";

								EntKairosParametrosProceso parametros = new() {
									IdNormaSuscrita = idNormaSuscrita,
									Cron = cronNuevo,
									ProgramarSiguienteEjecucion = cronVencimiento.Contains(cronNuevo)
								};

								SalKairosIngresarProceso retorno = await kairosHelper.IngresarProceso(new EntKairosIngresarProceso {
									Nombre = nombreProceso,
									Cron = cronNuevo,
									Parametros = JsonSerializer.Serialize(parametros, AppJsonSerializerContext.Default.EntKairosParametrosProceso),
									ArnProceso = variableEntornoHelper.Obtener("NOTIFICACIONES_LAMBDA_ARN"),
									ArnRol = variableEntornoHelper.Obtener("NOTIFICACIONES_EJECUCION_ROLE_ARN"),
									Habilitado = true,
								});
								procesosProgramados.Add(retorno.IdProceso);

								normaSuscrita.ProcesosNotificaciones ??= [];
								normaSuscrita.ProcesosNotificaciones.Add(new Dictionary<string, JsonElement> {
									["IdProceso"] = JsonSerializer.SerializeToElement(retorno.IdProceso, AppJsonSerializerContext.Default.String),
									["IdCalendarizacion"] = JsonSerializer.SerializeToElement(retorno.IdCalendarizacion, AppJsonSerializerContext.Default.String),
									["Nombre"] = JsonSerializer.SerializeToElement(retorno.Nombre, AppJsonSerializerContext.Default.String),
									["ArnRol"] = JsonSerializer.SerializeToElement(retorno.ArnRol, AppJsonSerializerContext.Default.String),
									["ArnProceso"] = JsonSerializer.SerializeToElement(retorno.ArnProceso, AppJsonSerializerContext.Default.String),
									["Parametros"] = JsonSerializer.SerializeToElement(retorno.Parametros, AppJsonSerializerContext.Default.String),
									["Habilitado"] = JsonSerializer.SerializeToElement(retorno.Habilitado, AppJsonSerializerContext.Default.Boolean),
									["FechaCreacion"] = JsonSerializer.SerializeToElement(retorno.FechaCreacion, AppJsonSerializerContext.Default.DateTime),
									["Cron"] = JsonSerializer.SerializeToElement(cronNuevo, AppJsonSerializerContext.Default.String)
								});
							}
						}

						await normaSuscritaDao.Actualizar(normaSuscrita, transaction);
					}
				}
			} catch {
				await Programar(procesosDesprogramados);
				await Desprogramar(procesosProgramados);
				throw;
			}
		}

		public async Task ProcesarNotificacion(long idNormaSuscrita, string cron, bool programarSiguienteEjecucion, NpgsqlTransaction? transaction = null) {
			// Se obtiene norma suscrita y/o template...
			NormaSuscrita normaSuscrita = await normaSuscritaDao.ObtenerPorId(idNormaSuscrita, transaction) ?? throw new Exception("ID norma suscrita inválida");
            TemplateNorma? templateNorma = null;
            if (normaSuscrita.IdTemplate != null && normaSuscrita.IdNorma != null) {
                templateNorma = (await templateNormaDao.ObtenerPorTemplate(normaSuscrita.IdTemplate.Value, transaction)).FirstOrDefault(n => n.IdNorma == normaSuscrita.IdNorma);
            }

			// Se obtienen destinatarios vigentes...
            List<DestinatarioNotificacion> destinatariosVigentes = await destinatarioNotificacionDao.ObtenerPorSub(normaSuscrita.Sub, normaSuscrita.IdNegocio, true, transaction);

			// Se obtienen los tipos de unidades de tiempo...
			List<TipoUnidadTiempo> tiposUnidadesTiempo = await tipoUnidadTiempoDao.ObtenerPorVigencia(true, transaction);

            // Si tenemos tipo de periodicidad, se continua con las notificaciones...
            if ((normaSuscrita.IdTipoPeriodicidad ?? templateNorma?.IdTipoPeriodicidad) != null) {
                TipoPeriodicidad tipoPeriodicidad = await tipoPeriodicidadDao.ObtenerPorId((normaSuscrita.IdTipoPeriodicidad ?? templateNorma?.IdTipoPeriodicidad!).Value, transaction) ?? throw new Exception("Tipo periodicidad inválido");
				if (!string.IsNullOrWhiteSpace(tipoPeriodicidad.Cron)) {
					
					// Se obtienen los historiales de norma suscrita que aún no se completan...
					List<HistorialNormaSuscrita> historialNormaSuscritas = await historialNormaSuscritaDao.ObtenerPorNormaSuscritaYFechaCompletitud(idNormaSuscrita, null, true, transaction);
					foreach (HistorialNormaSuscrita historialNormaSuscrita in historialNormaSuscritas) {
						
						// Se obtienen los historiales de notificación no ejecutados...
						List<HistorialNotificacion> historialNotificaciones = await historialNotificacionDao.ObtenerPorHistorial(historialNormaSuscrita.Id, null, true, transaction);
						foreach (HistorialNotificacion historialNotificacion in historialNotificaciones) {
							
							// Si el historial de notificación no corresponde al cron, se salta la notificación...
							string cronProgramacion = CronHelper.GenerarCronDesdeFecha(historialNotificacion.FechaProgramacion, tipoPeriodicidad.Cron);
							if (cronProgramacion != cron) {
								continue;
							}

							// Se valida que el destinatario este vigente, si no lo esta entonces no se manda la notificación...
							DestinatarioNotificacion? destinatario = destinatariosVigentes.FirstOrDefault(d => d.Id == historialNotificacion.IdDestinatarioNotificacion);
							if (destinatario == null) {
								continue;
							}

							// Se calcula la cantidad de tiempo faltante a vencimiento...
							string? tiempoFaltante = null;
							string? deLosProximos = null;
							if (historialNotificacion.IdTipoUnidadTiempoAntelacion != null && historialNotificacion.CantAntelacion != null) {
                                TipoUnidadTiempo? unidadTiempo = tiposUnidadesTiempo.FirstOrDefault(ut => ut.Id == historialNotificacion.IdTipoUnidadTiempoAntelacion);
								if (unidadTiempo == null) {
									continue;
								}

								tiempoFaltante = $"{historialNotificacion.CantAntelacion} {unidadTiempo.Nombre.ToLower()}";
								if (historialNotificacion.CantAntelacion > 1) tiempoFaltante += "s";

								if (historialNotificacion.CantAntelacion > 1) {
									if (historialNotificacion.IdTipoUnidadTiempoAntelacion == 1 ||
										historialNotificacion.IdTipoUnidadTiempoAntelacion == 3) {
                                        deLosProximos = $"de los próximos {historialNotificacion.CantAntelacion} {unidadTiempo.Nombre.ToLower()}s";
                                    } else {
                                        deLosProximos = $"de las próximas {historialNotificacion.CantAntelacion} {unidadTiempo.Nombre.ToLower()}s";
                                    }
                                } else {
									if (historialNotificacion.IdTipoUnidadTiempoAntelacion == 1) {
										deLosProximos = $"del próximo {unidadTiempo.Nombre.ToLower()}";
                                    } else if (historialNotificacion.IdTipoUnidadTiempoAntelacion == 3) {
										deLosProximos = $"de mañana";
                                    } else {
                                        deLosProximos = $"de la próxima {unidadTiempo.Nombre.ToLower()}";
                                    }
                                }
                            }

                            // Si el destinatario es email, se manda correo electrónico...
                            if (destinatario.IdTipoReceptor == 1) {
                                string strTemplateCorreo;
								string asunto;
								if (tiempoFaltante != null) {
                                    if (environment.IsProduction()) {
                                        strTemplateCorreo = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "TemplatesCorreos", "NotificacionPrevia.html"));
                                    } else {
                                        strTemplateCorreo = await File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), "TemplatesCorreos", "NotificacionPrevia.html"));
                                    }
									asunto = "¡Tu obligación vence en [TIEMPO_FALTANTE]!";
                                } else {
                                    if (environment.IsProduction()) {
                                        strTemplateCorreo = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "TemplatesCorreos", "NormaVencida.html"));
                                    } else {
                                        strTemplateCorreo = await File.ReadAllTextAsync(Path.Combine(Directory.GetCurrentDirectory(), "TemplatesCorreos", "NormaVencida.html"));
                                    }
									asunto = "¡Tu obligación venció!";

								}

                                await hermesHelper.EnviarCorreo(new EntHermesCorreoEnviar() {
                                    De = new DireccionCorreo() {
                                        Nombre = variableEntornoHelper.Obtener("HERMES_DE_NOMBRE"),
                                        Correo = variableEntornoHelper.Obtener("HERMES_DE_CORREO"),
                                    },
                                    Para = [
                                        new DireccionCorreo() {
											Correo = destinatario.Destino
										}
                                    ],
                                    Asunto = asunto.Replace("[TIEMPO_FALTANTE]", tiempoFaltante ?? ""),
                                    Cuerpo = strTemplateCorreo
                                                .Replace("[NOMBRE_NORMA]", WebUtility.HtmlEncode(normaSuscrita.Nombre ?? templateNorma?.Nombre ?? ""))
                                                .Replace("[MULTA_NORMA]", WebUtility.HtmlEncode(normaSuscrita.Multa ?? templateNorma?.Multa ?? ""))
                                                .Replace("[TIEMPO_FALTANTE]", WebUtility.HtmlEncode(tiempoFaltante ?? ""))
												.Replace("[DE_LOS_PROXIMOS]", WebUtility.HtmlEncode(deLosProximos ?? "")),
                                });

								historialNotificacion.FechaEjecucion = DateTime.UtcNow;
								await historialNotificacionDao.Actualizar(historialNotificacion, transaction);
                            }
                        }
                    }
				}
            }
		}

		public async Task Programar(List<EntKairosIngresarProceso> procesosProgramar) {
			foreach (EntKairosIngresarProceso proceso in procesosProgramar) {
				await kairosHelper.IngresarProceso(proceso);
			}
		}

		public async Task Desprogramar(List<string> idProcesosDesprogramar) {
			foreach (string idProceso in idProcesosDesprogramar) {
				await kairosHelper.EliminarProceso(idProceso);
			}
		}
    }
}
