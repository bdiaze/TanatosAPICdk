using Npgsql;
using System.Diagnostics;
using System.Text.Json;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others;
using TanatosAPI.Helpers;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class ProcesoNotificacionBcp(VariableEntornoHelper variableEntornoHelper, KairosHelper kairosHelper, NormaSuscritaDao normaSuscritaDao, TipoPeriodicidadDao tipoPeriodicidadDao, TipoUnidadTiempoDao tipoUnidadTiempoDao, HistorialNormaSuscritaDao historialNormaSuscritaDao, NotificacionNormaSuscritaDao notificacionNormaSuscritaDao, TemplateNormaNotificacionDao templateNormaNotificacionDao) {
		public async Task ActualizarProgramacionProcesosNormaSuscrita(long idNormaSuscrita, NpgsqlTransaction? transaction = null) { 
			NormaSuscrita normaSuscrita = await normaSuscritaDao.ObtenerPorId(idNormaSuscrita, transaction) ?? throw new Exception("Norma suscrita inválida");
			// Si la norma suscrita no está activada, se desprograman todas sus notificaciones...
			if (!normaSuscrita.Activado) {
				foreach (Dictionary<string, JsonElement> proceso in normaSuscrita.ProcesosNotificaciones ?? []) {
					if (proceso.TryGetValue("IdProceso", out JsonElement jsonIdProceso)) {
						string? idProceso = jsonIdProceso.ValueKind == JsonValueKind.String ? jsonIdProceso.GetString() : null;
						if (idProceso != null) {
							await kairosHelper.EliminarProceso(idProceso);
						}
					}
				}
				normaSuscrita.ProcesosNotificaciones = null;
				await normaSuscritaDao.Actualizar(normaSuscrita, transaction);

			// Si la norma suscrita está activada, se programan las notificaciones que no están programadas, y desprograman las que no son necesarias...
			} else if (normaSuscrita.IdTipoPeriodicidad != null) {
				TipoPeriodicidad tipoPeriodicidad = await tipoPeriodicidadDao.ObtenerPorId(normaSuscrita.IdTipoPeriodicidad.Value, transaction) ?? throw new Exception("Tipo periodicidad inválido");
				
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
							}
						}
						normaSuscrita.ProcesosNotificaciones!.Remove(eliminar);
					}

					// Se crean los procesos programados que faltan...
					string nombreProceso = $"{variableEntornoHelper.Obtener("APP_NAME")}-NormaSuscrita{idNormaSuscrita}";
					foreach (string cronNuevo in crons) {
						if (!cronsExistentes.Any(ce => ce == cronNuevo)) {
							EntKairosParametrosProceso parametros = new() { 
								IdNormaSuscrita = idNormaSuscrita,
								Cron = cronNuevo,
								ProgramarSiguienteEjecucion = cronVencimiento.Contains(cronNuevo)
							};

							SalKairosIngresarProceso retorno = await kairosHelper.IngresarProceso(new EntKairosIngresarProceso {
								Nombre = nombreProceso,
								Cron = cronNuevo,
								Habilitado = true,
								Parametros = JsonSerializer.Serialize(parametros, AppJsonSerializerContext.Default.EntKairosParametrosProceso),
								ArnProceso = variableEntornoHelper.Obtener("NOTIFICACIONES_LAMBDA_ARN"),
								ArnRol = variableEntornoHelper.Obtener("NOTIFICACIONES_EJECUCION_ROLE_ARN") 
							});

							normaSuscrita.ProcesosNotificaciones ??= [];
							normaSuscrita.ProcesosNotificaciones.Add(new Dictionary<string, JsonElement> {
								["IdProceso"] =  JsonSerializer.SerializeToElement(retorno.IdProceso, AppJsonSerializerContext.Default.String),
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
		}
	}
}
