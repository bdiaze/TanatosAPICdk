using Npgsql;
using System.Text.Json;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Kairos;

namespace TanatosAPI.Interfaces.Business {
	public interface INormaSuscritaBcp {
		public bool EstaVigente(NormaSuscrita? normaSuscrita);
		public bool Pertenece(NormaSuscrita normaSuscrita, string sub);
		public Task<NormaSuscrita?> ObtenerPorId(long idNormaSuscrita, NpgsqlTransaction? transaction = null);
		public Task Actualizar(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null);
		public Task Eliminar(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null);
		public Task ProgramarUnProcesoNotificacion(EntKairosIngresarProceso procesosProgramar);
		public Task ProgramarVariosProcesosNotificacion(List<EntKairosIngresarProceso> procesosProgramar);
		public Task DesprogramarUnProcesoNotificacion(string idProcesosDesprogramar);
		public Task DesprogramarVariosProcesosNotificacion(List<string> idProcesosDesprogramar);
		public Task ReversarProcesos(List<ProcesoNotificacion> procesosProgramados, List<ProcesoNotificacion> procesosDesprogramados);
        public Task<(List<ProcesoNotificacion> procesosCronProgramados, List<ProcesoNotificacion> procesosCronDesprogramados)> ActualizarProcesosCronProgramados(NormaSuscrita normaSuscrita, List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> cronsDeseados, NpgsqlTransaction? transaction = null);
		public Task<(List<ProcesoNotificacion> frecuenciasDiasProgramados, List<ProcesoNotificacion> frecuenciasDiasDesprogramadas)> ActualizarProcesosFrecuenciaDiasProgramados(NormaSuscrita normaSuscrita, List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> frecuenciasDiasDeseadas, NpgsqlTransaction? transaction = null);
    }
}
