using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;

namespace TanatosAPI.UseCases {
	public class NotificacionNormaSuscritaUseCase(INotificacionNormaSuscritaBcp notificacionNormaSuscritaBcp, ITemplateNormaNotificacionBcp templateNormaNotificacionBcp, ITipoUnidadTiempoBcp tipoUnidadTiempoBcp) {
		public async Task<List<(TipoUnidadTiempo UnidadTiempoAntelacion, int CantAntelacion)>> ObtenerAntelacionesConsiderandoTemplate(long idNormaSuscrita, long? idTemplate, long? idNormaTemplate, NpgsqlTransaction? transaction = null) {
			List<NotificacionNormaSuscrita> notificacionesNormaSuscrita = await notificacionNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(idNormaSuscrita, transaction);
			List<TemplateNormaNotificacion> templateNormaNotificacion = [];
			if (notificacionesNormaSuscrita.Count == 0 && idTemplate != null && idNormaTemplate != null) 
				templateNormaNotificacion = await templateNormaNotificacionBcp.ObtenerPorTemplateNorma(idTemplate.Value, idNormaTemplate.Value, transaction);

			HashSet<(long idTipoUnidadTiempo, int cantAntelacion)> antelaciones = [
				..notificacionNormaSuscritaBcp.ExtraerAntelaciones(notificacionesNormaSuscrita),
				..templateNormaNotificacionBcp.ExtraerAntelaciones(templateNormaNotificacion)
			];

			Dictionary<long, TipoUnidadTiempo> unidadesTiempo = (await tipoUnidadTiempoBcp.ObtenerVigentes(transaction)).ToDictionary(ut => ut.Id, ut => ut);

			return [.. antelaciones.Where(a => unidadesTiempo.ContainsKey(a.idTipoUnidadTiempo)).Select(a => (unidadesTiempo[a.idTipoUnidadTiempo] , a.cantAntelacion))];
		}

		public async Task<List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)>> GenerarCrons(DateTime proximoVencimientoUtc, string baseCronAws, List<(TipoUnidadTiempo TipoUnidadTiempo, int CantAntelacion)> antelaciones) {
			List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> crons = [];

			// Se añade primer cron correspondiente al vencimiento, sin info de antelación...
			DateTime proximoVencimientoChile = DateTimeHelper.TransformarFechaUTCATimezone(proximoVencimientoUtc);
			crons.Add((CronHelper.GenerarCronAWSDesdeFecha(proximoVencimientoChile, baseCronAws), null, null, true));

			// Por cada antelación, se calcula fecha de programación y se agrega cron respectivo...
			foreach ((TipoUnidadTiempo tipoUnidadTiempo, int cantAntelacion) in antelaciones) {
				DateTime fechaProgramacionChile = NotificacionPreviaHelper.ObtenerFechaChileNotificacionPrevia(proximoVencimientoChile, cantAntelacion, tipoUnidadTiempo);
				crons.Add((CronHelper.GenerarCronAWSDesdeFecha(fechaProgramacionChile, baseCronAws), tipoUnidadTiempo, cantAntelacion, false));
			}

			return crons;
		}

		public async Task<List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)>> GenerarFrecuenciasDias(DateTime proximoVencimientoUtc, int frecuenciaDias, List<(TipoUnidadTiempo TipoUnidadTiempo, int CantAntelacion)> antelaciones) {
			List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> frecuencias = [];

            // Se añade primera frecuencia correspondiente al vencimiento, sin info de antelación...
            DateTime proximoVencimientoChile = DateTimeHelper.TransformarFechaUTCATimezone(proximoVencimientoUtc);
            frecuencias.Add((frecuenciaDias, proximoVencimientoChile, null, null, true));

            // Por cada antelación, se calcula fecha de programación y se agrega cron respectivo...
            foreach ((TipoUnidadTiempo tipoUnidadTiempo, int cantAntelacion) in antelaciones) {
                DateTime fechaProgramacionChile = NotificacionPreviaHelper.ObtenerFechaChileNotificacionPrevia(proximoVencimientoChile, cantAntelacion, tipoUnidadTiempo);
                frecuencias.Add((frecuenciaDias, fechaProgramacionChile, tipoUnidadTiempo, cantAntelacion, false));
            }

            return frecuencias;
        }

    }
}
