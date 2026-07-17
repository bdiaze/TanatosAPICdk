using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class HistorialNormaSuscritaBcp(IDateTimeProvider dateTimeProvider, DocumentoAdjuntoBcp documentoAdjuntoBcp, INormaSuscritaDao normaSuscritaDao, IHistorialNormaSuscritaDao historialNormaSuscritaDao, ITemplateNormaDao templateNormaDao, ITipoPeriodicidadBcp tipoPeriodicidadBcp) {
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
		
		public async Task Crear(HistorialNormaSuscrita historialNormaSuscrita, NpgsqlTransaction? transaction = null) {
			historialNormaSuscrita.Id = await historialNormaSuscritaDao.Insertar(historialNormaSuscrita, transaction);
		}

		public async Task EliminarPorNormaSuscrita(NormaSuscrita normaSuscrita, bool ignorarVencidos = false, NpgsqlTransaction? transaction = null) {
			List<HistorialNormaSuscrita> historialesVigentes = await historialNormaSuscritaDao.ObtenerPorNormaSuscritaYFechaCompletitud(normaSuscrita.Id, null, true, transaction);

			if (ignorarVencidos) {
				historialesVigentes = [.. historialesVigentes.Where(h => h.FechaVencimiento > dateTimeProvider.UtcNow)];
			}

			foreach (HistorialNormaSuscrita historial in historialesVigentes) {
				historial.FechaEliminacion = dateTimeProvider.UtcNow;
				historial.Vigencia = false;
				await historialNormaSuscritaDao.Actualizar(historial, transaction);

				await documentoAdjuntoBcp.EliminarPorHistorialNormaSuscrita(historial.Id, transaction);
			}
		}

		public async Task CompletarHistorialNormaSuscrita(HistorialNormaSuscrita historialNormaSuscrita, NpgsqlTransaction? transaction = null) {
			if (historialNormaSuscrita.FechaCompletitud == null) {
				historialNormaSuscrita.FechaCompletitud = dateTimeProvider.UtcNow;
				await historialNormaSuscritaDao.Actualizar(historialNormaSuscrita, transaction);

				await ProgramarSiguienteVencimiento(historialNormaSuscrita, transaction);
			}
		}

		public async Task ProgramarSiguienteVencimiento(HistorialNormaSuscrita historialNormaSuscrita, NpgsqlTransaction? transaction = null) {
			// Solo se programa el siguiente vencimiento si no existe otro vencimiento futuro, no completado, distinto a la referencia...
			List<HistorialNormaSuscrita> historialesFuturos = [.. 
				(await historialNormaSuscritaDao.ObtenerPorNormaSuscrita(historialNormaSuscrita.IdNormaSuscrita, true, transaction))
					.Where(hns => hns.FechaCompletitud == null && hns.Id != historialNormaSuscrita.Id && hns.FechaVencimiento > dateTimeProvider.UtcNow)
			];
			if (historialesFuturos.Count > 0) {
				return;
			}

			// Se obtiene norma suscrita y/o template...
			NormaSuscrita normaSuscrita = await normaSuscritaDao.ObtenerPorId(historialNormaSuscrita.IdNormaSuscrita, transaction) ?? throw new InvalidOperationException("ID norma suscrita inválida");
			TemplateNorma? templateNorma = null;
			if (normaSuscrita.IdTemplate != null && normaSuscrita.IdNorma != null && normaSuscrita.IdTipoPeriodicidad == null) {
				templateNorma = (await templateNormaDao.ObtenerPorTemplate(normaSuscrita.IdTemplate.Value, transaction)).FirstOrDefault(n => n.IdNorma == normaSuscrita.IdNorma);
			}

			long idTipoPeriodicidad = (normaSuscrita.IdTipoPeriodicidad ?? templateNorma?.IdTipoPeriodicidad) ?? throw new InvalidOperationException("Tipo periodicidad inválido");
			TipoPeriodicidad tipoPeriodicidad = await tipoPeriodicidadBcp.ObtenerPorId(idTipoPeriodicidad, transaction) ?? throw new InvalidOperationException("Tipo periodicidad inválido");
			if (!string.IsNullOrWhiteSpace(tipoPeriodicidad.Cron)) {
				// Nos aseguramos de que la fecha esté en UTC...
				DateTime vencimientoActual = DateTime.SpecifyKind(historialNormaSuscrita.FechaVencimiento, DateTimeKind.Utc);

				// Se transforma la fecha de vencimiento actual a zona horaria de Chile...
				TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("America/Santiago");
				DateTime proximoVencimiento = TimeZoneInfo.ConvertTimeFromUtc(vencimientoActual, timeZoneInfo);

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
				proximoVencimiento = TimeZoneInfo.ConvertTimeToUtc(proximoVencimiento, timeZoneInfo);

				if (vencimientoActual != proximoVencimiento) {
					// Se crea el próximo vencimiento...
					HistorialNormaSuscrita nuevoHistorialNormaSuscrita = new() {
						Id = 0,
						IdNormaSuscrita = historialNormaSuscrita.IdNormaSuscrita,
						FechaVencimiento = proximoVencimiento,
						FechaCreacion = dateTimeProvider.UtcNow,
						Vigencia = true
					};

					await Crear(nuevoHistorialNormaSuscrita, transaction);
				}
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
