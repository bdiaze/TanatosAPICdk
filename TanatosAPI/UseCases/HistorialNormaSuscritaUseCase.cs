using Npgsql;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Interfaces.UseCases;
using TanatosAPI.Repositories;

namespace TanatosAPI.UseCases {
	public class HistorialNormaSuscritaUseCase(IDateTimeProvider dateTimeProvider, IHistorialNormaSuscritaBcp historialNormaSuscritaBcp, IDocumentoAdjuntoBcp documentoAdjuntoBcp, INormaSuscritaBcp normaSuscritaBcp, ITemplateNormaBcp templateNormaBcp, ITipoPeriodicidadBcp tipoPeriodicidadBcp) : IHistorialNormaSuscritaUseCase {
		public async Task EliminarPorNormaSuscrita(long idNormaSuscrita, bool ignorarVencidos, NpgsqlTransaction transaction) {
			List<HistorialNormaSuscrita> historialesVigentes = await historialNormaSuscritaBcp.ObtenerPorNormaSuscrita(idNormaSuscrita, filtrarVigente: true, filtrarNoCompletadas: true, transaction: transaction);

			if (ignorarVencidos) {
				historialesVigentes = [.. historialesVigentes.Where(h => h.FechaVencimiento > dateTimeProvider.UtcNow)];
			}

			foreach (HistorialNormaSuscrita historial in historialesVigentes) {
				await historialNormaSuscritaBcp.Eliminar(historial, transaction);
				await documentoAdjuntoBcp.EliminarPorHistorialNormaSuscrita(historial.Id, transaction);
			}
		}

		public async Task<DateTime> CompletarHistorialNormaSuscrita(HistorialNormaSuscrita historialNormaSuscrita, NpgsqlTransaction transaction) {
            DateTime? fechaCompletitud = historialNormaSuscrita.FechaCompletitud;
            if (fechaCompletitud == null) {
                fechaCompletitud = await historialNormaSuscritaBcp.Completar(historialNormaSuscrita, transaction);
				await ProgramarSiguienteVencimiento(historialNormaSuscrita, transaction);
			}
            return fechaCompletitud.Value;
		}

		public async Task ProgramarSiguienteVencimiento(HistorialNormaSuscrita historialNormaSuscrita, NpgsqlTransaction? transaction = null) {
			// Solo se programa el siguiente vencimiento si no existe otro vencimiento futuro, no completado, distinto a la referencia...
			bool yaTieneVencimiento = await historialNormaSuscritaBcp.TieneVencimientoFuturoNoCompletado(
				historialNormaSuscrita.IdNormaSuscrita, 
				[ historialNormaSuscrita.Id ], 
				transaction
			);
            if (yaTieneVencimiento) return;

			// Se obtiene norma suscrita y/o template...
			NormaSuscrita normaSuscrita = await normaSuscritaBcp.Obtener(historialNormaSuscrita.IdNormaSuscrita, transaction: transaction) ?? throw new InvalidOperationException("ID norma suscrita inválida");
			TemplateNorma? templateNorma = null;
			if (normaSuscrita.IdTemplate != null && normaSuscrita.IdNorma != null && normaSuscrita.IdTipoPeriodicidad == null) {
				templateNorma = await templateNormaBcp.ObtenerPorTemplateNorma(normaSuscrita.IdTemplate.Value, normaSuscrita.IdNorma.Value, transaction);
			}

			TipoPeriodicidad tipoPeriodicidad = await tipoPeriodicidadBcp.ObtenerValidandoVigencia(
                normaSuscrita.IdTipoPeriodicidad ?? templateNorma?.IdTipoPeriodicidad,
				transaction
            );

			DateTime proximoVencimiento = CalcularSiguienteVencimiento(historialNormaSuscrita.FechaVencimiento, tipoPeriodicidad);

			if (historialNormaSuscrita.FechaVencimiento != proximoVencimiento) {
				// Se crea el próximo vencimiento...
				_ = await historialNormaSuscritaBcp.Crear(historialNormaSuscrita.IdNormaSuscrita, proximoVencimiento, transaction);
			}
		}

        public DateTime CalcularSiguienteVencimiento(DateTime vencimientoActual, TipoPeriodicidad tipoPeriodicidad, bool fechasChilenas = false) {
			tipoPeriodicidadBcp.ValidarDeltas(tipoPeriodicidad);

            DateTime proximoVencimiento = !fechasChilenas ? DateTimeHelper.TransformarFechaUTCATimezone(vencimientoActual) : vencimientoActual;

            // Se añaden los deltas de la periodicidad...
            if (tipoPeriodicidad.DeltaDias != null) {
                proximoVencimiento = proximoVencimiento.AddDays(tipoPeriodicidad.DeltaDias.Value);
            } else if (tipoPeriodicidad.DeltaMeses != null) {
                proximoVencimiento = proximoVencimiento.AddMonths(tipoPeriodicidad.DeltaMeses.Value);
            } else if (tipoPeriodicidad.DeltaAnnos != null) {
                proximoVencimiento = proximoVencimiento.AddYears(tipoPeriodicidad.DeltaAnnos.Value);
            }

            // Se convierte próximo vencimiento calculado a UTC...
            return !fechasChilenas ? DateTimeHelper.TransformarFechaTimezoneAUTC(proximoVencimiento) : proximoVencimiento;
        }

        public DateTime CalcularVencimientoFuturo(DateTime fechaReferenciaUTC, TipoPeriodicidad tipoPeriodicidad) {
            // Si la fecha de refencia ya es futura, se devuelve esa misma...
            DateTime nowUtc = dateTimeProvider.UtcNow;
            if (fechaReferenciaUTC > nowUtc) return fechaReferenciaUTC;

            tipoPeriodicidadBcp.ValidarDeltas(tipoPeriodicidad);

            DateTime fechaReferenciaChile = DateTimeHelper.TransformarFechaUTCATimezone(fechaReferenciaUTC);
            DateTime fechaActualChile = DateTimeHelper.TransformarFechaUTCATimezone(nowUtc);

            if (tipoPeriodicidad.DeltaAnnos != null) {
                int fraccion = (int)Math.Floor((double)(fechaActualChile.Year - fechaReferenciaChile.Year) / tipoPeriodicidad.DeltaAnnos.Value);
                fechaReferenciaChile = fechaReferenciaChile.AddYears(tipoPeriodicidad.DeltaAnnos.Value * fraccion);
            } else if (tipoPeriodicidad.DeltaMeses != null) {
                int diffMeses = ((fechaActualChile.Year - fechaReferenciaChile.Year) * 12) + (fechaActualChile.Month - fechaReferenciaChile.Month);
                int fraccion = (int)Math.Floor((double)diffMeses / tipoPeriodicidad.DeltaMeses.Value);
                fechaReferenciaChile = fechaReferenciaChile.AddMonths(tipoPeriodicidad.DeltaMeses.Value * fraccion);
            } else if (tipoPeriodicidad.DeltaDias != null) {
                int fraccion = (int)Math.Floor((fechaActualChile - fechaReferenciaChile).TotalDays / tipoPeriodicidad.DeltaDias.Value);
                fechaReferenciaChile = fechaReferenciaChile.AddDays(tipoPeriodicidad.DeltaDias.Value * fraccion);
            }

            while (fechaReferenciaChile <= fechaActualChile) {
                fechaReferenciaChile = CalcularSiguienteVencimiento(fechaReferenciaChile, tipoPeriodicidad, true);
            }

            return DateTimeHelper.TransformarFechaTimezoneAUTC(fechaReferenciaChile);
        }
    }
}
