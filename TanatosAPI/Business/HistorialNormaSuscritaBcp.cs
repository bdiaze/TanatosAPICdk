using Npgsql;
using System.Transactions;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class HistorialNormaSuscritaBcp(IDateTimeProvider dateTimeProvider, IHistorialNormaSuscritaDao historialNormaSuscritaDao) : IHistorialNormaSuscritaBcp {
		public bool EstaVigente(HistorialNormaSuscrita? historialNormaSuscrita) {
			return historialNormaSuscrita != null && historialNormaSuscrita.Vigencia;
		}

		public bool EstaCompletada(HistorialNormaSuscrita historialNormaSuscrita) {
			return historialNormaSuscrita.FechaCompletitud != null;
        }

		public bool VigenteOCompletada(HistorialNormaSuscrita? historialNormaSuscrita) {
			return EstaVigente(historialNormaSuscrita) || (historialNormaSuscrita != null && EstaCompletada(historialNormaSuscrita));
		}

		public async Task<HistorialNormaSuscrita?> ObtenerPorId(long idHistorialNormaSuscrita) {
			return await historialNormaSuscritaDao.ObtenerPorId(idHistorialNormaSuscrita);
		}
		
		public async Task<List<HistorialNormaSuscrita>> ObtenerVigentesPorNormaSuscritaNoCompletadas(long idNormaSuscrita, NpgsqlTransaction? transaction = null) {
			return await historialNormaSuscritaDao.ObtenerPorNormaSuscritaYFechaCompletitud(idNormaSuscrita, null, true, transaction);
		}

		public async Task<DateTime> ObtenerProximoVencimiento(long idNormaSuscrita, NpgsqlTransaction? transaction = null) {
			HistorialNormaSuscrita proximoVencimiento = (await historialNormaSuscritaDao.ObtenerPorNormaSuscritaYFechaCompletitud(idNormaSuscrita, null, true, transaction))
				.OrderByDescending(v => v.FechaVencimiento)
				.FirstOrDefault() ?? throw new InvalidOperationException("La obligación no cuenta con un vencimiento no completado.");
			return proximoVencimiento.FechaVencimiento;
		}

		public async Task<HistorialNormaSuscrita> Crear(long idNormaSuscrita, DateTime fechaVencimiento, NpgsqlTransaction? transaction = null) {
			HistorialNormaSuscrita nuevo = new() { 
				Id = 0,
				IdNormaSuscrita = idNormaSuscrita,
				FechaVencimiento = fechaVencimiento,
				FechaCompletitud = null,
				FechaCreacion = dateTimeProvider.UtcNow,
				FechaEliminacion = null,
				Vigencia = true
			};
			nuevo.Id = await historialNormaSuscritaDao.Insertar(nuevo, transaction);
			return nuevo;
		}

		public async Task Eliminar(HistorialNormaSuscrita historialNormaSuscrita, NpgsqlTransaction? transaction = null) {
			if (historialNormaSuscrita.Vigencia) {
				historialNormaSuscrita.FechaEliminacion = dateTimeProvider.UtcNow;
				historialNormaSuscrita.Vigencia = false;
				await historialNormaSuscritaDao.Actualizar(historialNormaSuscrita, transaction);
			}
		}

		public async Task Completar(HistorialNormaSuscrita historialNormaSuscrita, NpgsqlTransaction? transaction = null) {
			if (historialNormaSuscrita.FechaCompletitud == null) {
				historialNormaSuscrita.FechaCompletitud = dateTimeProvider.UtcNow;
				await historialNormaSuscritaDao.Actualizar(historialNormaSuscrita, transaction);
			}
		}
				
		public DateTime CalcularVencimientoFuturo(DateTime fechaReferenciaUTC, TipoPeriodicidad tipoPeriodicidad) {
			// Nos aseguramos de que la fecha esté en UTC...
			if (fechaReferenciaUTC.Kind != DateTimeKind.Utc)
				throw new InvalidOperationException("La fecha de referencia debe ser UTC");

			// Si la fecha de refencia ya es futura, se devuelve esa misma...
			DateTime nowUtc = dateTimeProvider.UtcNow;
			if (fechaReferenciaUTC > nowUtc) return fechaReferenciaUTC;

			// Si el tipo periodicidad no tiene deltas o tienes múltiples deltas, se lanza excepción...
			int cantDeltas =
				(tipoPeriodicidad.DeltaAnnos != null ? 1 : 0) +
				(tipoPeriodicidad.DeltaMeses != null ? 1 : 0) +
				(tipoPeriodicidad.DeltaDias != null ? 1 : 0);

			if (cantDeltas == 0) 
				throw new InvalidOperationException($"No se puede calcular vencimiento futuro para este tipo de periodicidad - ID Tipo Periodicidad: {tipoPeriodicidad.Id}");

			if (cantDeltas > 1)
				throw new InvalidOperationException($"El tipo de periodicidad tiene múltiples deltas definidos - ID Tipo Periodicidad: {tipoPeriodicidad.Id}");

			if (tipoPeriodicidad.DeltaAnnos != null && tipoPeriodicidad.DeltaAnnos <= 0)
				throw new InvalidOperationException($"El tipo de periodicidad tiene un delta de años inválido - ID Tipo Periodicidad: {tipoPeriodicidad.Id}");

			if (tipoPeriodicidad.DeltaMeses != null && tipoPeriodicidad.DeltaMeses <= 0)
				throw new InvalidOperationException($"El tipo de periodicidad tiene un delta de meses inválido - ID Tipo Periodicidad: {tipoPeriodicidad.Id}");

			if (tipoPeriodicidad.DeltaDias != null && tipoPeriodicidad.DeltaDias <= 0)
				throw new InvalidOperationException($"El tipo de periodicidad tiene un delta de dias inválido - ID Tipo Periodicidad: {tipoPeriodicidad.Id}");

			TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("America/Santiago");

			DateTime fechaReferenciaChile = TimeZoneInfo.ConvertTimeFromUtc(fechaReferenciaUTC, timeZoneInfo);
			DateTime fechaActualChile = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, timeZoneInfo);

			if (tipoPeriodicidad.DeltaAnnos != null) {
				int fraccion = (int)Math.Floor((double)(fechaActualChile.Year - fechaReferenciaChile.Year) / tipoPeriodicidad.DeltaAnnos.Value);
				fechaReferenciaChile = fechaReferenciaChile.AddYears(tipoPeriodicidad.DeltaAnnos.Value * fraccion);
			}

			if (tipoPeriodicidad.DeltaMeses != null) {
				int diffMeses = ((fechaActualChile.Year - fechaReferenciaChile.Year) * 12) + (fechaActualChile.Month - fechaReferenciaChile.Month);
				int fraccion = (int)Math.Floor((double)diffMeses / tipoPeriodicidad.DeltaMeses.Value);
				fechaReferenciaChile = fechaReferenciaChile.AddMonths(tipoPeriodicidad.DeltaMeses.Value * fraccion);
			}

			if (tipoPeriodicidad.DeltaDias != null) {
				int fraccion = (int)Math.Floor((fechaActualChile - fechaReferenciaChile).TotalDays / tipoPeriodicidad.DeltaDias.Value);
				fechaReferenciaChile = fechaReferenciaChile.AddDays(tipoPeriodicidad.DeltaDias.Value * fraccion);
			}
			
			while (fechaReferenciaChile <= fechaActualChile) {
				if (tipoPeriodicidad.DeltaAnnos != null) {
					fechaReferenciaChile = fechaReferenciaChile.AddYears(tipoPeriodicidad.DeltaAnnos.Value);
				}
				if (tipoPeriodicidad.DeltaMeses != null) {
					fechaReferenciaChile = fechaReferenciaChile.AddMonths(tipoPeriodicidad.DeltaMeses.Value);
				}
				if (tipoPeriodicidad.DeltaDias != null) {
					fechaReferenciaChile = fechaReferenciaChile.AddDays(tipoPeriodicidad.DeltaDias.Value);
				}
			}

			return TimeZoneInfo.ConvertTimeToUtc(fechaReferenciaChile, timeZoneInfo);
		}
	}
}
