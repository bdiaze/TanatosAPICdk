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
	public class NormaSuscritaUseCase(HistorialNormaSuscritaUseCase historialNormaSuscritaUseCase, NotificacionNormaSuscritaUseCase notificacionNormaSuscritaUseCase, INormaSuscritaBcp normaSuscritaBcp, ITemplateNormaBcp templateNormaBcp, IHistorialNormaSuscritaBcp historialNormaSuscritaBcp, IFiscalizadorNormaSuscritaBcp fiscalizadorNormaSuscritaBcp, INotificacionNormaSuscritaBcp notificacionNormaSuscritaBcp, ITipoPeriodicidadBcp tipoPeriodicidadBcp) {
		public async Task<(NormaSuscrita?, TemplateNorma?)> ObtenerPorIdConTemplate(long idNormaSuscrita, NpgsqlTransaction? transaction = null) {
			NormaSuscrita? normaSuscrita = await normaSuscritaBcp.ObtenerPorId(idNormaSuscrita, transaction);
			TemplateNorma? templateNorma = null;
			if (normaSuscrita?.IdTemplate != null && normaSuscrita?.IdNorma != null) {
				templateNorma = await templateNormaBcp.ObtenerPorTemplateNorma(normaSuscrita.IdTemplate.Value, normaSuscrita.IdNorma.Value, transaction);
			}
			return (normaSuscrita, templateNorma);
		}
				
		public async Task ActualizarProgramacionProcesosNormaSuscrita(long idNormaSuscrita, NpgsqlTransaction? transaction = null) {
			List<ProcesoNotificacion> procesosProgramados = [];
			List<ProcesoNotificacion> procesosDesprogramados = [];
			try {
				(NormaSuscrita? normaSuscrita, TemplateNorma? templateNorma) = await ObtenerPorIdConTemplate(idNormaSuscrita, transaction);
				if (normaSuscrita == null) throw new InvalidOperationException("Norma suscrita inválida");

				List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> cronsDeseados = [];
				List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> frecuenciasDiasDeseadas = [];

                // Si la norma suscrita está activada, se obtienen los crons y frecuencias deseados...
                if (normaSuscrita.Activado) {
					long idTipoPeriodicidad = (normaSuscrita.IdTipoPeriodicidad ?? templateNorma?.IdTipoPeriodicidad) ?? throw new InvalidOperationException("Obligación sin tipo de periodicidad asociada");
					TipoPeriodicidad? tipoPeriodicidad = await tipoPeriodicidadBcp.ObtenerPorId(idTipoPeriodicidad, transaction);

					// Solo se obtienen los crons y frecuencias deseados si la periodicidad está vigente...
					if (tipoPeriodicidadBcp.EstaVigente(tipoPeriodicidad)) {
						// Se obtienen las notificaciones previas configuradas para la norma suscrita...
						List<(TipoUnidadTiempo UnidadTiempo, int CantAntelacion)> antelaciones = await notificacionNormaSuscritaUseCase.ObtenerAntelacionesConsiderandoTemplate(normaSuscrita.Id, normaSuscrita.IdTemplate, normaSuscrita.IdNorma, transaction);
							
						// Se calculan los crons y frecuencias según el próximo vencimiento...
						DateTime proximoVencimiento = await historialNormaSuscritaBcp.ObtenerProximoVencimiento(normaSuscrita.Id, transaction);
						if (!string.IsNullOrWhiteSpace(tipoPeriodicidad!.Cron)) {
							cronsDeseados = await notificacionNormaSuscritaUseCase.GenerarCrons(
								proximoVencimiento,
								tipoPeriodicidad!.Cron,
								antelaciones
							);
						} else if (tipoPeriodicidad!.FrecuenciaDias != null) {
                            frecuenciasDiasDeseadas = await notificacionNormaSuscritaUseCase.GenerarFrecuenciasDias(
                                proximoVencimiento,
                                tipoPeriodicidad!.FrecuenciaDias.Value,
                                antelaciones
                            );
                        }
					}
				}

                (List<ProcesoNotificacion> cronsProgramados, List<ProcesoNotificacion> cronsDesprogramados) = await normaSuscritaBcp.ActualizarProcesosCronProgramados(normaSuscrita, cronsDeseados);
                procesosProgramados.AddRange(cronsProgramados);
                procesosDesprogramados.AddRange(cronsDesprogramados);

				(List<ProcesoNotificacion> frecuenciasDiasProgramados, List<ProcesoNotificacion> frecuenciasDiasDesprogramadas) = await normaSuscritaBcp.ActualizarProcesosFrecuenciaDiasProgramados(normaSuscrita, frecuenciasDiasDeseadas);
                procesosProgramados.AddRange(frecuenciasDiasProgramados);
                procesosDesprogramados.AddRange(frecuenciasDiasDesprogramadas);
			} catch {
				await normaSuscritaBcp.ReversarProcesos(procesosProgramados, procesosDesprogramados);
				throw;
			}
		}

		public async Task EliminarNormaSuscrita(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null) {
			if (normaSuscrita.Vigencia) {
				await normaSuscritaBcp.Eliminar(normaSuscrita);

				await ActualizarProgramacionProcesosNormaSuscrita(normaSuscrita.Id, transaction);

				await fiscalizadorNormaSuscritaBcp.EliminarPorNormaSuscrita(normaSuscrita.Id, transaction);
				await notificacionNormaSuscritaBcp.EliminarPorNormaSuscrita(normaSuscrita.Id, transaction);
				await historialNormaSuscritaUseCase.EliminarPorNormaSuscrita(normaSuscrita.Id, false, transaction);
			}
		}
	}
}
