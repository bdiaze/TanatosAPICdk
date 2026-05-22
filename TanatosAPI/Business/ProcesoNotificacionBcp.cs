using Cronos;
using Microsoft.AspNetCore.SignalR;
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
using TimeZoneConverter;
using static Google.Rpc.Context.AttributeContext.Types;

namespace TanatosAPI.Business {
	public class ProcesoNotificacionBcp(IHostEnvironment environment, VariableEntornoHelper variableEntornoHelper, HermesHelper hermesHelper, KairosHelper kairosHelper, CryptoHelper cryptoHelper, CognitoHelper cognitoHelper, DestinatarioNotificacionBcp destinatarioNotificacionBcp, HistorialNormaSuscritaBcp historialNormaSuscritaBcp, SuscripcionBcp suscripcionBcp, NormaSuscritaDao normaSuscritaDao, TipoPeriodicidadDao tipoPeriodicidadDao, TipoUnidadTiempoDao tipoUnidadTiempoDao, HistorialNormaSuscritaDao historialNormaSuscritaDao, HistorialNotificacionDao historialNotificacionDao, NotificacionNormaSuscritaDao notificacionNormaSuscritaDao, TemplateNormaDao templateNormaDao, TemplateNormaNotificacionDao templateNormaNotificacionDao, DestinatarioNotificacionDao destinatarioNotificacionDao) {
		private const int DIAS_CADUCIDAD_CODIGO_ACCESO = 30;

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
						Dictionary<string, (long? IdTipoUnidadTiempoAntelacion, int? CantAntelacion)> crons = [];
						List<HistorialNormaSuscrita> historialNormaSuscritas = [.. (await historialNormaSuscritaDao.ObtenerPorNormaSuscritaYFechaCompletitud(idNormaSuscrita, null, true, transaction)).Where(hns => hns.FechaVencimiento > DateTime.UtcNow)];
						foreach (HistorialNormaSuscrita historialNormaSuscrita in historialNormaSuscritas) {
							cronVencimiento.Add(CronHelper.GenerarCronAWSDesdeFecha(CronHelper.TransformarFechaUTCATimezone(historialNormaSuscrita.FechaVencimiento), tipoPeriodicidad.Cron));

							foreach ((long? IdTipoUnidadTiempoAntelacion, int? CantAntelacion) antelacion in configNotifPrevias) {
								DateTime fechaProgramacion = CronHelper.TransformarFechaUTCATimezone(historialNormaSuscrita.FechaVencimiento);

								if (antelacion.IdTipoUnidadTiempoAntelacion != null && antelacion.CantAntelacion != null) {
									TipoUnidadTiempo? tipoUnidadTiempo = tiposUnidadTiempo.FirstOrDefault(ut => ut.Id == antelacion.IdTipoUnidadTiempoAntelacion);
									if (tipoUnidadTiempo != null) {
										fechaProgramacion = NotificacionPreviaHelper.ObtenerFechaChileNotificacionPrevia(fechaProgramacion, antelacion.CantAntelacion.Value, tipoUnidadTiempo);
									} else {
										continue;
									}
								}

								crons.Add(CronHelper.GenerarCronAWSDesdeFecha(fechaProgramacion, tipoPeriodicidad.Cron), (antelacion.IdTipoUnidadTiempoAntelacion, antelacion.CantAntelacion));
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
							if (!crons.Keys.Any(c => c == cronExistente)) {
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
						foreach (string cronNuevo in crons.Keys) {
							if (!cronsExistentes.Any(ce => ce == cronNuevo)) {
								string nombreProceso = $"{variableEntornoHelper.Obtener("APP_NAME")} - NormaSuscrita {idNormaSuscrita} - Cron {cronNuevo}";

								EntKairosParametrosProceso parametros = new() {
									IdNormaSuscrita = idNormaSuscrita,
									Cron = cronNuevo,
									IdTipoUnidadTiempoAntelacion = crons[cronNuevo].IdTipoUnidadTiempoAntelacion,
									CantAntelacion = crons[cronNuevo].CantAntelacion,
									EsVencimiento = cronVencimiento.Contains(cronNuevo),
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

		public async Task ProcesarNotificacion(long idNormaSuscrita, string cron, long? idTipoUnidadTiempoAntelacion, int? cantAntelacion, bool? esVencimiento, bool programarSiguienteEjecucion, NpgsqlTransaction? transaction = null) {
			esVencimiento ??= programarSiguienteEjecucion;

            // Se obtiene norma suscrita y/o template...
            NormaSuscrita normaSuscrita = await normaSuscritaDao.ObtenerPorId(idNormaSuscrita, transaction) ?? throw new Exception("ID norma suscrita inválida");
            TemplateNorma? templateNorma = null;
            if (normaSuscrita.IdTemplate != null && normaSuscrita.IdNorma != null) {
                templateNorma = (await templateNormaDao.ObtenerPorTemplate(normaSuscrita.IdTemplate.Value, transaction)).FirstOrDefault(n => n.IdNorma == normaSuscrita.IdNorma);
            }

			// Se obtienen destinatarios vigentes...
			List<DestinatarioNotificacion> destinatariosVigentes = await destinatarioNotificacionDao.ObtenerPorSub(normaSuscrita.Sub, normaSuscrita.IdNegocio, true, transaction);
			List<DestinatarioNotificacion> destinatariosValidados = [.. destinatariosVigentes.Where(dn => dn.Validado)];

			// Se valida que exista un destinatario correspondiente a la cuenta del usuario...
			Dictionary<string, string> atributosUsuario = await cognitoHelper.ObtenerUsuario(normaSuscrita.Sub);
			string? correoUsuario = atributosUsuario.TryGetValue("email", out string? email) ? email : null;
			if (correoUsuario != null && !destinatariosValidados.Any(d => d.IdEmpleado == null && d.IdTipoReceptor == 1 /* Correo electrónico */ && d.Destino == correoUsuario)) {
				DestinatarioNotificacion nuevoDestinatario = await destinatarioNotificacionBcp.Crear(
					normaSuscrita.Sub,
					normaSuscrita.IdNegocio,
					null,
					1, // Correo electrónico
					"Mi correo electrónico",
					correoUsuario,
					true,
					transaction
				);
				destinatariosValidados.Add(nuevoDestinatario);
			}

			// Se obtienen los tipos de unidades de tiempo...
			List<TipoUnidadTiempo> tiposUnidadesTiempo = await tipoUnidadTiempoDao.ObtenerPorVigencia(true, transaction);
            TipoUnidadTiempo? unidadTiempo = tiposUnidadesTiempo.FirstOrDefault(ut => ut.Id == idTipoUnidadTiempoAntelacion);
						
			string timezone = "America/Santiago";
			if (OperatingSystem.IsWindows()) {
				timezone = TZConvert.IanaToWindows(timezone);
			}
			TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timezone);

			// Se calcula la fecha a la que corresponde la ejecución actual, según la ocurrencia del cron más cercana...
			CronExpression cronExpression = CronExpression.Parse(CronHelper.TransformarCronAWSAStandard(cron));
			DateTime utcNow = DateTime.UtcNow;
			DateTime? siguienteUTC = cronExpression.GetNextOccurrence(utcNow, timeZoneInfo);
			DateTime? anteriorUTC = cronExpression.GetPreviousOccurrence(utcNow, timeZoneInfo, true);
			DateTime masCercanaUTC = (siguienteUTC, anteriorUTC) switch {
				(null, null) => throw new InvalidOperationException($"El cron '{cron}' no tiene ocurrencias válidas."),
				(null, _) => anteriorUTC!.Value,
				(_, null) => siguienteUTC!.Value,
				_ => (siguienteUTC!.Value - utcNow) <= (utcNow - anteriorUTC!.Value) ? siguienteUTC!.Value : anteriorUTC!.Value
			};

			// Se calcula el vencimiento al que corresponde la ejecución actual...
			HistorialNormaSuscrita? vencimiento = null;
			if (cantAntelacion != null && idTipoUnidadTiempoAntelacion != null && unidadTiempo != null) {
				// Si tenemos información de la notificación previa, se calculca la fecha de vencimiento...
				DateTime masCercanaChile = CronHelper.TransformarFechaUTCATimezone(masCercanaUTC);
				DateTime fechaVencimientoChile = NotificacionPreviaHelper.ObtenerFechaReferenciaChileSegunNotificacionPrevia(masCercanaChile, cantAntelacion.Value, unidadTiempo);
				DateTime fechaVencimientoUTC = CronHelper.TransformarFechaTimezoneAUTC(fechaVencimientoChile);

				vencimiento = (await historialNormaSuscritaDao.ObtenerPorNormaSuscritaYFechaCompletitud(idNormaSuscrita, null, true, transaction)).FirstOrDefault(v => v.FechaVencimiento == fechaVencimientoUTC);
			} else if (!esVencimiento.Value) {
				// Si no estamos en una fecha de vencimiento, pero tampoco tenemos información de la notificación previa, se asume último vencimiento...
				vencimiento = (await historialNormaSuscritaDao.ObtenerPorNormaSuscritaYFechaCompletitud(idNormaSuscrita, null, true, transaction)).OrderByDescending(v => v.FechaVencimiento).FirstOrDefault();

			} else {
				// Si estamos en una fecha de vencimiento, se busca el vencimiento que coincide con la fecha del cron...
				vencimiento = (await historialNormaSuscritaDao.ObtenerPorNormaSuscritaYFechaCompletitud(idNormaSuscrita, null, true, transaction)).FirstOrDefault(v => v.FechaVencimiento == masCercanaUTC);
			}

			if (vencimiento != null) {
                // Se definen textos a incluirse en la notificación...
                string? tiempoFaltante = null;
                string? deLosProximos = null;
                if (cantAntelacion != null && idTipoUnidadTiempoAntelacion != null && unidadTiempo != null) {
                    if (cantAntelacion > 1) tiempoFaltante = $"{cantAntelacion} {unidadTiempo.NombrePlural?.ToLower()}";
                    else tiempoFaltante = $"{cantAntelacion} {unidadTiempo.Nombre.ToLower()}";

                    if (cantAntelacion > 1) {
                        if (idTipoUnidadTiempoAntelacion == 1 || idTipoUnidadTiempoAntelacion == 3) {
                            deLosProximos = $"de los próximos {cantAntelacion} {unidadTiempo.NombrePlural?.ToLower()}";
                        } else {
                            deLosProximos = $"de las próximas {cantAntelacion} {unidadTiempo.NombrePlural?.ToLower()}";
                        }
                    } else {
                        if (idTipoUnidadTiempoAntelacion == 1) {
                            deLosProximos = $"del próximo {unidadTiempo.Nombre.ToLower()}";
                        } else if (idTipoUnidadTiempoAntelacion == 3) {
                            deLosProximos = $"de mañana";
                        } else {
                            deLosProximos = $"de la próxima {unidadTiempo.Nombre.ToLower()}";
                        }
                    }
                } else if (!esVencimiento.Value) {
                    tiempoFaltante = "poco tiempo";
                    deLosProximos = $"del {vencimiento.FechaVencimiento:dd 'de' MMMM}";
                }

                // Se procesan las notificaciones de todos los destinatarios validados...
                foreach (DestinatarioNotificacion destinatario in destinatariosValidados) {
                    HistorialNotificacion historialNotificacion = new() {
                        Id = 0,
                        IdHistorialNormaSuscrita = vencimiento.Id,
						IdDestinatarioNotificacion = destinatario.Id,
						IdTipoUnidadTiempoAntelacion = idTipoUnidadTiempoAntelacion,
						CantAntelacion = cantAntelacion,
						FechaProgramacion = masCercanaUTC,
						FechaCreacion = DateTime.UtcNow,
						Vigencia = true
                    };
                    historialNotificacion.Id = await historialNotificacionDao.Insertar(historialNotificacion, transaction);

                    // Se valida que según suscripción el destinatario esté habilitado, si no lo esta entonces no se manda la notificación...
                    if (!await suscripcionBcp.DestinatarioHabilitado(normaSuscrita.Sub, normaSuscrita.IdNegocio, destinatario.Id, transaction)) {
                        historialNotificacion.FechaEjecucion = DateTime.UtcNow;
                        historialNotificacion.Estado = 2; // Omitido
                        historialNotificacion.Observacion = "El destinatario no está habilitado según la suscripción del usuario.";
                        await historialNotificacionDao.Actualizar(historialNotificacion, transaction);

                        continue;
                    }

					// Se valida que la unidad de tiempo este vigente, solo si viene como entrada...
                    if (idTipoUnidadTiempoAntelacion != null && unidadTiempo == null) {
                        historialNotificacion.FechaEjecucion = DateTime.UtcNow;
                        historialNotificacion.Estado = 2; // Omitido
                        historialNotificacion.Observacion = "El tipo de unidad de tiempo no está vigente.";
                        await historialNotificacionDao.Actualizar(historialNotificacion, transaction);

                        continue;
                    }
					
                    // Se genera código de acceso para notificación...
                    string codigoAcceso = cryptoHelper.GenerarToken();
                    HistorialNotificacion? mismoCodigo = await historialNotificacionDao.ObtenerPorCodigoAcceso(cryptoHelper.HashSHA256(codigoAcceso), true, transaction);
                    while (mismoCodigo != null) {
                        codigoAcceso = cryptoHelper.GenerarToken();
                        mismoCodigo = await historialNotificacionDao.ObtenerPorCodigoAcceso(cryptoHelper.HashSHA256(codigoAcceso), true, transaction);
                    }

                    // Si el destinatario es email, se manda correo electrónico...
                    if (destinatario.IdTipoReceptor == 1) {
                        string strTemplateCorreo;
                        string asunto;
                        if (!esVencimiento.Value) {
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

                        SalHermesEnviar response = await hermesHelper.EnviarCorreo(new EntHermesCorreoEnviar() {
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
                                        .Replace("[NOMBRE_NORMA]", WebUtility.HtmlEncode(normaSuscrita.Nombre ?? templateNorma?.Nombre ?? "Sin nombre registrado"))
                                        .Replace("[MULTA_NORMA]", WebUtility.HtmlEncode(normaSuscrita.Multa ?? templateNorma?.Multa ?? "Sin multa registrada"))
                                        .Replace("[TIEMPO_FALTANTE]", WebUtility.HtmlEncode(tiempoFaltante ?? ""))
                                        .Replace("[DE_LOS_PROXIMOS]", WebUtility.HtmlEncode(deLosProximos ?? ""))
                                        .Replace("[CODIGO_ACCESO]", Uri.EscapeDataString(codigoAcceso))
                        });

                        historialNotificacion.FechaEjecucion = DateTime.UtcNow;
                        historialNotificacion.Estado = 1; // Enviado
                        historialNotificacion.CodigoAcceso = cryptoHelper.HashSHA256(codigoAcceso);
                        historialNotificacion.FechaCaducidadCodigoAcceso = DateTime.UtcNow.AddDays(DIAS_CADUCIDAD_CODIGO_ACCESO);
                        historialNotificacion.HermesIdMensaje = response.IdMensaje;
                        await historialNotificacionDao.Actualizar(historialNotificacion, transaction);

					// Si el destinatario es Whatsapp, se manda mensaje de Whatsapp...
                    } else if (destinatario.IdTipoReceptor == 2) {
                        string nombreTemplate;
                        string[]? parametrosTitulo;
                        string[]? parametrosCuerpo;
                        if (!esVencimiento.Value) {
                            nombreTemplate = "notificacion_previa";
                            parametrosTitulo = [
                                tiempoFaltante!
                            ];
                            parametrosCuerpo = [
                                normaSuscrita.Nombre ?? templateNorma?.Nombre ?? "Sin nombre registrado",
                                deLosProximos!,
                                normaSuscrita.Multa ?? templateNorma?.Multa ?? "Sin multa registrada"
                            ];
                        } else {
                            nombreTemplate = "norma_vencida";
                            parametrosTitulo = null;
                            parametrosCuerpo = [
                                normaSuscrita.Nombre ?? templateNorma?.Nombre ?? "Sin nombre registrado",
                                normaSuscrita.Multa ?? templateNorma?.Multa ?? "Sin multa registrada"
                            ];
                        }

                        SalHermesEnviar response = await hermesHelper.EnviarWhatsapp(new EntHermesWhatsappEnviar() {
                            De = variableEntornoHelper.Obtener("HERMES_DE_WHATSAPP"),
                            Para = destinatario.Destino,
                            NombreTemplate = nombreTemplate,
                            ParametrosTitulo = parametrosTitulo,
                            ParametrosCuerpo = parametrosCuerpo,
                            ParametrosBoton = [Uri.EscapeDataString(codigoAcceso)]
                        });

                        historialNotificacion.FechaEjecucion = DateTime.UtcNow;
                        historialNotificacion.Estado = 1; // Enviado
                        historialNotificacion.CodigoAcceso = cryptoHelper.HashSHA256(codigoAcceso);
                        historialNotificacion.FechaCaducidadCodigoAcceso = DateTime.UtcNow.AddDays(DIAS_CADUCIDAD_CODIGO_ACCESO);
                        historialNotificacion.HermesIdMensaje = response.IdMensaje;
                        await historialNotificacionDao.Actualizar(historialNotificacion, transaction);
                    
					// En cualquier otro caso, se omite la notificación por falta de implementación...
                    } else {
                        historialNotificacion.FechaEjecucion = DateTime.UtcNow;
                        historialNotificacion.Estado = 2; // Omitido
                        historialNotificacion.Observacion = "El tipo de receptor asociado al destinatario no tiene lógica de notificación implementada.";
                        await historialNotificacionDao.Actualizar(historialNotificacion, transaction);
                    }
                }

                if (programarSiguienteEjecucion) {
					await historialNormaSuscritaBcp.ProgramarSiguienteVencimiento(vencimiento, transaction);
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
