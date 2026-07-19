using Cronos;
using Microsoft.AspNetCore.Components.RenderTree;
using Npgsql;
using Scriban.Runtime;
using System.Net;
using System.Text.Json;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Hermes;
using TanatosAPI.Entities.Others.Kairos;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.UseCases {
	public class NormaSuscritaUseCase(IDateTimeProvider dateTimeProvider, IVariableEntornoHelper variableEntornoHelper, IKairosHelper kairosHelper, INormaSuscritaBcp normaSuscritaBcp, ITemplateNormaBcp templateNormaBcp, HistorialNormaSuscritaBcp historialNormaSuscritaBcp, INormaSuscritaDao normaSuscritaDao, ITipoPeriodicidadBcp tipoPeriodicidadBcp, ITipoUnidadTiempoDao tipoUnidadTiempoDao, IHistorialNormaSuscritaDao historialNormaSuscritaDao, ITemplateNormaNotificacionDao templateNormaNotificacionDao, IFiscalizadorNormaSuscritaBcp fiscalizadorNormaSuscritaBcp, INotificacionNormaSuscritaBcp notificacionNormaSuscritaBcp) {
		public async Task<(NormaSuscrita?, TemplateNorma?)> ObtenerPorIdConTemplate(long idNormaSuscrita, NpgsqlTransaction? transaction = null) {
			NormaSuscrita? normaSuscrita = await normaSuscritaBcp.ObtenerPorId(idNormaSuscrita, transaction);
			TemplateNorma? templateNorma = null;
			if (normaSuscrita?.IdTemplate != null && normaSuscrita?.IdNorma != null) {
				templateNorma = await templateNormaBcp.ObtenerPorTemplateNorma(normaSuscrita.IdTemplate.Value, normaSuscrita.IdNorma.Value, transaction);
			}
			return (normaSuscrita, templateNorma);
		}

		
		public async Task ActualizarProgramacionProcesosNormaSuscrita(long idNormaSuscrita, NpgsqlTransaction? transaction = null) {
			List<string> procesosProgramados = [];
			List<EntKairosIngresarProceso> procesosDesprogramados = [];
			try {
				(NormaSuscrita? normaSuscrita, TemplateNorma? templateNorma) = await ObtenerPorIdConTemplate(idNormaSuscrita, transaction);
				if (normaSuscrita == null) throw new InvalidOperationException("Norma suscrita inválida");

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
					long idTipoPeriodicidad = (normaSuscrita.IdTipoPeriodicidad ?? templateNorma?.IdTipoPeriodicidad) ?? throw new InvalidOperationException("Tipo periodicidad inválido");
					TipoPeriodicidad tipoPeriodicidad = await tipoPeriodicidadBcp.ObtenerPorId(idTipoPeriodicidad, transaction) ?? throw new InvalidOperationException("Tipo periodicidad inválido");

					if (!string.IsNullOrWhiteSpace(tipoPeriodicidad.Cron)) {
						// Se arma listado de las configuraciones de notificaciones previas...
						HashSet<(long? IdTipoUnidadTiempoAntelacion, int? CantAntelacion)> configNotifPrevias = [];
						configNotifPrevias.Add((null, null));

						List<NotificacionNormaSuscrita> notificacionesNormaSuscrita = await notificacionNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(idNormaSuscrita, transaction);
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

						// Se arman los cron a programar según los próximos vencimientos...
						HashSet<string> cronVencimiento = [];
						Dictionary<string, (long? IdTipoUnidadTiempoAntelacion, int? CantAntelacion)> crons = [];

						HistorialNormaSuscrita? proximoVencimiento = (await historialNormaSuscritaDao.ObtenerPorNormaSuscritaYFechaCompletitud(idNormaSuscrita, null, true, transaction)).OrderByDescending(v => v.FechaVencimiento).FirstOrDefault();
						if (proximoVencimiento != null) {
							cronVencimiento.Add(CronHelper.GenerarCronAWSDesdeFecha(DateTimeHelper.TransformarFechaUTCATimezone(proximoVencimiento.FechaVencimiento), tipoPeriodicidad.Cron));

							foreach ((long? IdTipoUnidadTiempoAntelacion, int? CantAntelacion) antelacion in configNotifPrevias) {
								DateTime fechaProgramacionChile = DateTimeHelper.TransformarFechaUTCATimezone(proximoVencimiento.FechaVencimiento);

								if (antelacion.IdTipoUnidadTiempoAntelacion != null && antelacion.CantAntelacion != null) {
									TipoUnidadTiempo? tipoUnidadTiempo = tiposUnidadTiempo.FirstOrDefault(ut => ut.Id == antelacion.IdTipoUnidadTiempoAntelacion);
									if (tipoUnidadTiempo != null) {
										fechaProgramacionChile = NotificacionPreviaHelper.ObtenerFechaChileNotificacionPrevia(fechaProgramacionChile, antelacion.CantAntelacion.Value, tipoUnidadTiempo);
									} else {
										continue;
									}
								}

								crons.Add(CronHelper.GenerarCronAWSDesdeFecha(fechaProgramacionChile, tipoPeriodicidad.Cron), (antelacion.IdTipoUnidadTiempoAntelacion, antelacion.CantAntelacion));
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

		public async Task EliminarNormaSuscrita(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null) {
			if (normaSuscrita.Vigencia) {
				DateTime utcNow = dateTimeProvider.UtcNow;
				if (normaSuscrita.Activado) {
					normaSuscrita.FechaDesactivacion = utcNow;
					normaSuscrita.Activado = false;
				}

				normaSuscrita.FechaEliminacion = utcNow;
				normaSuscrita.Vigencia = false;
				await normaSuscritaDao.Actualizar(normaSuscrita, transaction);
				await ActualizarProgramacionProcesosNormaSuscrita(normaSuscrita.Id, transaction);


				await fiscalizadorNormaSuscritaBcp.EliminarPorNormaSuscrita(normaSuscrita.Id, transaction);
				await notificacionNormaSuscritaBcp.EliminarPorNormaSuscrita(normaSuscrita.Id, transaction);
				await historialNormaSuscritaBcp.EliminarPorNormaSuscrita(normaSuscrita, false, transaction);
			}
		}
	}
}
