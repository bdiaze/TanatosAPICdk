using Microsoft.AspNetCore.SignalR;
using Npgsql;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.UseCases {
	public class TemplateNormaUseCase(NormaSuscritaUseCase normaSuscritaUseCase, INotificacionNormaSuscritaBcp notificacionNormaSuscritaBcp, IFiscalizadorNormaSuscritaBcp fiscalizadorNormaSuscritaBcp, ITemplateNormaDao templateNormaDao, ITemplateNormaNotificacionBcp templateNormaNotificacionBcp, ITemplateNormaFiscalizadorDao templateNormaFiscalizadorDao, INormaSuscritaDao normaSuscritaDao) {
		public async Task<(List<ProcesoNotificacion> procesosProgramados, List<ProcesoNotificacion> procesosDesprogramados)> Eliminar(long idTemplate, long? idNorma, IDatabaseTransaction transaction) {
			List<ProcesoNotificacion> procesosProgramados = [];
			List<ProcesoNotificacion> procesosDesprogramados = [];

			Dictionary<long, TemplateNorma> templateNormas = (await templateNormaDao.ObtenerPorTemplate(idTemplate, transaction!.NpgsqlTransaction())).ToDictionary(tn => tn.IdNorma, tn => tn);
			Dictionary<long, HashSet<(long IdTipoUnidadTiempoAntelacion, int CantAntelacion)>> templateNormasNotificaciones =
				(await templateNormaNotificacionBcp.ObtenerPorTemplateNorma(idTemplate, idNorma, transaction!.NpgsqlTransaction()))
				.GroupBy(tnn => tnn.IdNorma)
				.ToDictionary(tnn => tnn.Key, tnn => tnn.Select(x => (x.IdTipoUnidadTiempoAntelacion, x.CantAntelacion)).ToHashSet());
			Dictionary<long, HashSet<long>> templateNormasFiscalizadores =
				(await templateNormaFiscalizadorDao.ObtenerPorTemplateNorma(idTemplate, idNorma, transaction!.NpgsqlTransaction()))
				.GroupBy(tnf => tnf.IdNorma)
				.ToDictionary(tnf => tnf.Key, tnf => tnf.Select(x => x.IdTipoFiscalizador).ToHashSet());

			// Previo a eliminar el template norma, se desenlazan todas las normas suscritas relacionadas...
			List<NormaSuscrita> normasSuscritasDependientes = await normaSuscritaDao.ObtenerPorTemplate(idTemplate, idNorma, null, transaction!.NpgsqlTransaction());
			foreach (NormaSuscrita normaSuscrita in normasSuscritasDependientes) {
				// Si no tiene notificaciones, se dejan las del template norma...
				List<NotificacionNormaSuscrita> notificacionNormaSuscritas = await notificacionNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(normaSuscrita.Id, transaction!.NpgsqlTransaction());
				if (notificacionNormaSuscritas.Count == 0 && templateNormasNotificaciones.TryGetValue(normaSuscrita.IdNorma!.Value, out HashSet<(long IdTipoUnidadTiempoAntelacion, int CantAntelacion)>? templateNormaNotificacion)) {
					await notificacionNormaSuscritaBcp.ActualizarPorNormaSuscrita(normaSuscrita.Id, templateNormaNotificacion, transaction!.NpgsqlTransaction());
				}

				// Si no tiene fiscalizadores, se dejan los del template norma...
				List<FiscalizadorNormaSuscrita> fiscalizadorNormaSuscritas = await fiscalizadorNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(normaSuscrita.Id, transaction!.NpgsqlTransaction());
				if (fiscalizadorNormaSuscritas.Count == 0 && templateNormasFiscalizadores.TryGetValue(normaSuscrita.IdNorma!.Value, out HashSet<long>? templateNormaFiscalizador)) {
					await fiscalizadorNormaSuscritaBcp.ActualizarPorNormaSuscrita(normaSuscrita.Id, templateNormaFiscalizador, transaction!.NpgsqlTransaction());
				}

				TemplateNorma templateNorma = templateNormas[normaSuscrita.IdNorma!.Value];

				normaSuscrita.Nombre ??= templateNorma.Nombre;
				normaSuscrita.Descripcion ??= templateNorma.Descripcion;
				normaSuscrita.IdTipoPeriodicidad ??= templateNorma.IdTipoPeriodicidad;
				normaSuscrita.Multa ??= templateNorma.Multa;
				normaSuscrita.IdCategoriaNorma ??= templateNorma.IdCategoriaNorma;
				normaSuscrita.Editable = true;
				normaSuscrita.IdTemplate = null;
				normaSuscrita.IdNorma = null;

				// Si la norma suscrita no está activada se elimina
				if (!normaSuscrita.Activado) {
					(List<ProcesoNotificacion> programadosParcial, List<ProcesoNotificacion> desprogramadosParcial) = await normaSuscritaUseCase.EliminarNormaSuscrita(normaSuscrita, transaction);
					procesosProgramados.AddRange(programadosParcial);
					procesosDesprogramados.AddRange(desprogramadosParcial);

					// Pero si está activada, solo se desenlaza del template
				} else {
					await normaSuscritaDao.Actualizar(normaSuscrita, transaction!.NpgsqlTransaction());
				}
			}

			await templateNormaNotificacionBcp.Eliminar(idTemplate, idNorma, null, null, transaction!.NpgsqlTransaction());
			await templateNormaFiscalizadorDao.Eliminar(idTemplate, idNorma, null, transaction!.NpgsqlTransaction());
			await templateNormaDao.Eliminar(idTemplate, idNorma, transaction!.NpgsqlTransaction());

			return (procesosProgramados, procesosDesprogramados);
		}
	}
}
