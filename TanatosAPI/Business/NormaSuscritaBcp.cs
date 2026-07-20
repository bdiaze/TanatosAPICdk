using Npgsql;
using System.Globalization;
using System.Text.Json;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Kairos;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class NormaSuscritaBcp(INormaSuscritaDao normaSuscritaDao, IKairosHelper kairosHelper) : INormaSuscritaBcp {
		public bool EstaVigente(NormaSuscrita? normaSuscrita) {
			return normaSuscrita != null && normaSuscrita.Vigencia;
		}

		public bool Pertenece(NormaSuscrita normaSuscrita, string sub) {
			return normaSuscrita.Sub == sub;
		}

		public async Task ProgramarUnProcesoNotificacion(EntKairosIngresarProceso procesosProgramar) {
			await kairosHelper.IngresarProceso(procesosProgramar);
		}

		public async Task ProgramarVariosProcesosNotificacion(List<EntKairosIngresarProceso> procesosProgramar) {
			foreach (EntKairosIngresarProceso proceso in procesosProgramar) {
				await ProgramarUnProcesoNotificacion(proceso);
			}
		}

		public async Task DesprogramarUnProcesoNotificacion(string idProcesosDesprogramar) {
			await kairosHelper.EliminarProceso(idProcesosDesprogramar);
		}

		public async Task DesprogramarVariosProcesosNotificacion(List<string> idProcesosDesprogramar) {
			foreach (string idProceso in idProcesosDesprogramar) {
				await DesprogramarUnProcesoNotificacion(idProceso);
			}
		}

		public (string?, EntKairosIngresarProceso?) DictJsonElementAEntKairosIngresarProceso(Dictionary<string, JsonElement> proceso) {
			if (proceso.TryGetValue("IdProceso", out JsonElement jsonIdProceso)) {
				string? idProceso = (jsonIdProceso.ValueKind == JsonValueKind.String) ? jsonIdProceso.GetString() : null;
				if (idProceso != null) {
					if (proceso.TryGetValue("Nombre", out JsonElement jsonNombre) && jsonNombre.ValueKind == JsonValueKind.String &&
						proceso.TryGetValue("ArnRol", out JsonElement jsonArnRol) && jsonArnRol.ValueKind == JsonValueKind.String &&
						proceso.TryGetValue("ArnProceso", out JsonElement jsonArnProceso) && jsonArnProceso.ValueKind == JsonValueKind.String &&
						proceso.TryGetValue("Parametros", out JsonElement jsonParametros) && jsonParametros.ValueKind == JsonValueKind.String &&
						proceso.TryGetValue("Habilitado", out JsonElement jsonHabilitado) && (jsonHabilitado.ValueKind == JsonValueKind.True || jsonHabilitado.ValueKind == JsonValueKind.False)) {

						string? cron = null;
						if (proceso.TryGetValue("Cron", out JsonElement jsonCron) && jsonCron.ValueKind == JsonValueKind.String) {
							cron = jsonCron.GetString();
						}

						int? frecuenciaDias = null;
						if (proceso.TryGetValue("FrecuenciaDias", out JsonElement jsonFrecuenciaDias) && jsonFrecuenciaDias.ValueKind == JsonValueKind.Number) {
							frecuenciaDias = jsonFrecuenciaDias.GetInt32();
						}

						DateTime? inicioEjecucionUtc = null;
						if (proceso.TryGetValue("InicioEjecucionUtc", out JsonElement jsonInicioEjecucionUtc) && jsonInicioEjecucionUtc.ValueKind == JsonValueKind.String) {
							inicioEjecucionUtc = DateTime.ParseExact(
								jsonInicioEjecucionUtc.GetString()!,
								"O",
								CultureInfo.InvariantCulture,
								DateTimeStyles.RoundtripKind
							);
						}

						if (cron == null && frecuenciaDias == null) throw new InvalidOperationException("El diccionario no contiene cron ni frecuencia en días.");
						if (cron != null && frecuenciaDias != null) throw new InvalidOperationException("El diccionario no puede contener cron y frecuencia en días.");
						if (frecuenciaDias != null && inicioEjecucionUtc == null) throw new InvalidOperationException("El diccionario no contiene inicio de ejecución.");

						return (
							idProceso,
							new EntKairosIngresarProceso() {
								Nombre = jsonNombre.GetString()!,
								Cron = cron,
								FrecuenciaDias = frecuenciaDias,
								InicioEjecucionUtc = inicioEjecucionUtc,
								ArnRol = jsonArnRol.GetString()!,
								ArnProceso = jsonArnProceso.GetString()!,
								Parametros = jsonParametros.GetString()!,
								Habilitado = jsonHabilitado.GetBoolean()
							}
						);
					}
				}	
			}

			throw new InvalidOperationException("El diccionario no contiene todos los elementos para armar entrada de Kairos");
		}

		public Dictionary<string, JsonElement> SalKairosIngresarProcesoADictJsonElement(SalKairosIngresarProceso proceso, string? cron, int? frecuenciaDias, DateTime? inicioEjecucionUtc) {
			return new() {
				["IdProceso"] = JsonSerializer.SerializeToElement(proceso.IdProceso, AppJsonSerializerContext.Default.String),
				["IdCalendarizacion"] = JsonSerializer.SerializeToElement(proceso.IdCalendarizacion, AppJsonSerializerContext.Default.String),
				["Nombre"] = JsonSerializer.SerializeToElement(proceso.Nombre, AppJsonSerializerContext.Default.String),
				["ArnRol"] = JsonSerializer.SerializeToElement(proceso.ArnRol, AppJsonSerializerContext.Default.String),
				["ArnProceso"] = JsonSerializer.SerializeToElement(proceso.ArnProceso, AppJsonSerializerContext.Default.String),
				["Parametros"] = JsonSerializer.SerializeToElement(proceso.Parametros, AppJsonSerializerContext.Default.String),
				["Habilitado"] = JsonSerializer.SerializeToElement(proceso.Habilitado, AppJsonSerializerContext.Default.Boolean),
				["FechaCreacion"] = JsonSerializer.SerializeToElement(proceso.FechaCreacion, AppJsonSerializerContext.Default.DateTime),
				["Cron"] = JsonSerializer.SerializeToElement(cron, AppJsonSerializerContext.Default.String),
				["FrecuenciaDias"] = JsonSerializer.SerializeToElement(frecuenciaDias, AppJsonSerializerContext.Default.NullableInt32),
				["InicioEjecucionUtc"] = JsonSerializer.SerializeToElement(inicioEjecucionUtc?.ToString("O", CultureInfo.InvariantCulture), AppJsonSerializerContext.Default.String)
			};
		}

		public async Task<(List<(string? cron, int? frecuenciaDias, DateTime? inicioEjecucionUtc)> procesosProgramados, List<EntKairosIngresarProceso> procesosDesprogramados)> ActualizarProcesosProgramados(NormaSuscrita normaSuscrita, HashSet<(string? cron, int? frecuenciaDias, DateTime? inicioEjecucionUtc)> procesos) {
			List<(string? cron, int? frecuenciaDias, DateTime? inicioEjecucionUtc)> procesosProgramados = [];
			List<EntKairosIngresarProceso> procesosDesprogramados = [];

			// Se obtiene los procesos existentes...



			return (procesosProgramados, procesosDesprogramados);
		}

		public async Task<NormaSuscrita?> ObtenerPorId(long idNormaSuscrita, NpgsqlTransaction? transaction = null) {
            return await normaSuscritaDao.ObtenerPorId(idNormaSuscrita, transaction);
        }
	}
}
