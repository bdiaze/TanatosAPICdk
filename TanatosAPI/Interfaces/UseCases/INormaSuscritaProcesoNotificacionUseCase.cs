using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Kairos;
using TanatosAPI.Interfaces.Helpers;

namespace TanatosAPI.Interfaces.UseCases {
	public interface INormaSuscritaProcesoNotificacionUseCase {
		public List<NormaSuscritaProcesoNotificacion> ExtraerCronsAEliminar(List<NormaSuscritaProcesoNotificacion> procesosNotificacion, List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> cronsDeseados);
		public List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> ExtraerCronsACrear(List<NormaSuscritaProcesoNotificacion> procesosNotificacion, List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> cronsDeseados);
		public List<NormaSuscritaProcesoNotificacion> ExtraerFrecuenciasDiasAEliminar(List<NormaSuscritaProcesoNotificacion> procesosNotificacion, List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> frecuenciasDiasDeseadas);
		public List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> ExtraerFrecuenciasDiasACrear(List<NormaSuscritaProcesoNotificacion> procesosNotificacion, List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> frecuenciasDiasDeseadas);
		public Task<List<NormaSuscritaProcesoNotificacion>> ObtenerPorNormaSuscrita(long idNormaSuscrita, bool filtrarVigente = false, NpgsqlTransaction? transaction = null);
		public Task<NormaSuscritaProcesoNotificacion> RegistrarProcesoNotificacion(long idNormaSuscrita, string idProcesoKairos, string idCalendarizacionKairos, string nombre, string arnRol, string arnProceso, string parametros, string? cron, int? frecuenciaDias, DateTime? inicioEjecucionUtc, IDatabaseTransaction? transaction = null);
		public Task EliminarProcesoNotificacion(NormaSuscritaProcesoNotificacion normaSuscritaProceso, IDatabaseTransaction? transaction = null);
		public Task ReversarProcesosProgramadosDesprogramados(List<SalKairosIngresarProceso> procesosProgramados, List<NormaSuscritaProcesoNotificacion> procesosDesprogramados);
		public Task<(List<SalKairosIngresarProceso> procesosCronProgramados, List<NormaSuscritaProcesoNotificacion> procesosCronDesprogramados)> ActualizarProcesosNotificacionesCron(NormaSuscrita normaSuscrita, List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> cronsDeseados, IDatabaseTransaction? transaction = null);
		public Task<(List<SalKairosIngresarProceso> frecuenciasDiasProgramados, List<NormaSuscritaProcesoNotificacion> frecuenciasDiasDesprogramadas)> ActualizarProcesosNotificacionesFrecuenciaDias(NormaSuscrita normaSuscrita, List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> frecuenciasDiasDeseadas, IDatabaseTransaction? transaction = null);
		public Task<(List<SalKairosIngresarProceso> procesosProgramados, List<NormaSuscritaProcesoNotificacion> procesosDesprogramados)> ActualizarProcesosNotificacionesNormaSuscrita(NormaSuscrita normaSuscrita, List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> cronsDeseados, List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> frecuenciasDiasDeseadas, IDatabaseTransaction? transaction = null);
	}
}
