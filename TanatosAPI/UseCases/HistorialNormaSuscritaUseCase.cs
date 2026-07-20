using Npgsql;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.UseCases {
	public class HistorialNormaSuscritaUseCase(IDateTimeProvider dateTimeProvider, IHistorialNormaSuscritaBcp historialNormaSuscritaBcp, IDocumentoAdjuntoBcp documentoAdjuntoBcp, INormaSuscritaBcp normaSuscritaBcp, ITemplateNormaBcp templateNormaBcp, ITipoPeriodicidadBcp tipoPeriodicidadBcp) {
		public async Task EliminarPorNormaSuscrita(long idNormaSuscrita, bool ignorarVencidos = false, NpgsqlTransaction? transaction = null) {
			List<HistorialNormaSuscrita> historialesVigentes = await historialNormaSuscritaBcp.ObtenerVigentesPorNormaSuscritaNoCompletadas(idNormaSuscrita, transaction);

			if (ignorarVencidos) {
				historialesVigentes = [.. historialesVigentes.Where(h => h.FechaVencimiento > dateTimeProvider.UtcNow)];
			}

			foreach (HistorialNormaSuscrita historial in historialesVigentes) {
				await historialNormaSuscritaBcp.Eliminar(historial, transaction);
				await documentoAdjuntoBcp.EliminarPorHistorialNormaSuscrita(historial.Id, transaction);
			}
		}

		public async Task CompletarHistorialNormaSuscrita(HistorialNormaSuscrita historialNormaSuscrita, NpgsqlTransaction? transaction = null) {
			if (historialNormaSuscrita.FechaCompletitud == null) {
				await historialNormaSuscritaBcp.Completar(historialNormaSuscrita, transaction);
				await ProgramarSiguienteVencimiento(historialNormaSuscrita, transaction);
			}
		}

		public async Task ProgramarSiguienteVencimiento(HistorialNormaSuscrita historialNormaSuscrita, NpgsqlTransaction? transaction = null) {
			// Solo se programa el siguiente vencimiento si no existe otro vencimiento futuro, no completado, distinto a la referencia...
			List<HistorialNormaSuscrita> historialesFuturos = [..
				(await historialNormaSuscritaBcp.ObtenerVigentesPorNormaSuscritaNoCompletadas(historialNormaSuscrita.IdNormaSuscrita, transaction))
					.Where(hns => hns.Id != historialNormaSuscrita.Id && hns.FechaVencimiento > dateTimeProvider.UtcNow)
			];
			if (historialesFuturos.Count > 0) {
				return;
			}

			// Se obtiene norma suscrita y/o template...
			NormaSuscrita normaSuscrita = await normaSuscritaBcp.ObtenerPorId(historialNormaSuscrita.IdNormaSuscrita, transaction) ?? throw new InvalidOperationException("ID norma suscrita inválida");
			TemplateNorma? templateNorma = null;
			if (normaSuscrita.IdTemplate != null && normaSuscrita.IdNorma != null && normaSuscrita.IdTipoPeriodicidad == null) {
				templateNorma = (await templateNormaBcp.ObtenerPorTemplate(normaSuscrita.IdTemplate.Value, transaction)).FirstOrDefault(n => n.IdNorma == normaSuscrita.IdNorma);
			}

			long idTipoPeriodicidad = (normaSuscrita.IdTipoPeriodicidad ?? templateNorma?.IdTipoPeriodicidad) ?? throw new InvalidOperationException("Tipo periodicidad inválido");
			TipoPeriodicidad tipoPeriodicidad = await tipoPeriodicidadBcp.ObtenerPorId(idTipoPeriodicidad, transaction) ?? throw new InvalidOperationException("Tipo periodicidad inválido");
			if (!tipoPeriodicidad.Vigencia) throw new InvalidOperationException("Tipo periodicidad inválido");

			// Nos aseguramos de que la fecha esté en UTC...
			DateTime vencimientoActual = DateTime.SpecifyKind(historialNormaSuscrita.FechaVencimiento, DateTimeKind.Utc);

			// Se transforma la fecha de vencimiento actual a zona horaria de Chile...
			DateTime proximoVencimiento = DateTimeHelper.TransformarFechaUTCATimezone(vencimientoActual);

			// Se añaden los deltas de la periodicidad...
			if (tipoPeriodicidad.DeltaDias != null) {
				proximoVencimiento = proximoVencimiento.AddDays(tipoPeriodicidad.DeltaDias.Value);
			}
			if (tipoPeriodicidad.DeltaMeses != null) {
				proximoVencimiento = proximoVencimiento.AddMonths(tipoPeriodicidad.DeltaMeses.Value);
			}
			if (tipoPeriodicidad.DeltaAnnos != null) {
				proximoVencimiento = proximoVencimiento.AddYears(tipoPeriodicidad.DeltaAnnos.Value);
			}

			// Se convierte próximo vencimiento calculado a UTC...
			proximoVencimiento = DateTimeHelper.TransformarFechaTimezoneAUTC(proximoVencimiento);

			if (vencimientoActual != proximoVencimiento) {
				// Se crea el próximo vencimiento...
				_ = await historialNormaSuscritaBcp.Crear(historialNormaSuscrita.IdNormaSuscrita, proximoVencimiento, transaction);
			}
		}
	}
}
