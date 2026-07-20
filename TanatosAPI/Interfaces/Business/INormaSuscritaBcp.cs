using Npgsql;
using System.Text.Json;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Kairos;

namespace TanatosAPI.Interfaces.Business {
	public interface INormaSuscritaBcp {
		public bool EstaVigente(NormaSuscrita? normaSuscrita);
		public bool Pertenece(NormaSuscrita normaSuscrita, string sub);
		public Task ProgramarUnProcesoNotificacion(EntKairosIngresarProceso procesosProgramar);
		public Task ProgramarVariosProcesosNotificacion(List<EntKairosIngresarProceso> procesosProgramar);
		public Task DesprogramarUnProcesoNotificacion(string idProcesosDesprogramar);
		public Task DesprogramarVariosProcesosNotificacion(List<string> idProcesosDesprogramar);
		public (string?, EntKairosIngresarProceso?) DictJsonElementAEntKairosIngresarProceso(Dictionary<string, JsonElement> proceso);
		public Dictionary<string, JsonElement> SalKairosIngresarProcesoADictJsonElement(SalKairosIngresarProceso proceso, string? cron, int? frecuenciaDias, DateTime? inicioEjecucionUtc);
		public Task<NormaSuscrita?> ObtenerPorId(long idNormaSuscrita, NpgsqlTransaction? transaction = null);
	}
}
