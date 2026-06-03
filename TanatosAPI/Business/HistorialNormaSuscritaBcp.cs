using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class HistorialNormaSuscritaBcp(DocumentoAdjuntoBcp documentoAdjuntoBcp, NormaSuscritaDao normaSuscritaDao, HistorialNormaSuscritaDao historialNormaSuscritaDao, TemplateNormaDao templateNormaDao, TipoPeriodicidadDao tipoPeriodicidadDao) {
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
				historialesVigentes = [.. historialesVigentes.Where(h => h.FechaVencimiento > DateTime.UtcNow)];
			}

			foreach (HistorialNormaSuscrita historial in historialesVigentes) {
				historial.FechaEliminacion = DateTime.UtcNow;
				historial.Vigencia = false;
				await historialNormaSuscritaDao.Actualizar(historial, transaction);

				await documentoAdjuntoBcp.EliminarPorHistorialNormaSuscrita(historial.Id, transaction);
			}
		}

		public async Task CompletarHistorialNormaSuscrita(HistorialNormaSuscrita historialNormaSuscrita, NpgsqlTransaction? transaction = null) {
			if (historialNormaSuscrita.FechaCompletitud == null) {
				historialNormaSuscrita.FechaCompletitud = DateTime.UtcNow;
				await historialNormaSuscritaDao.Actualizar(historialNormaSuscrita, transaction);

				await ProgramarSiguienteVencimiento(historialNormaSuscrita, transaction);
			}
		}

		public async Task ProgramarSiguienteVencimiento(HistorialNormaSuscrita historialNormaSuscrita, NpgsqlTransaction? transaction = null) {
			// Solo se programa el siguiente vencimiento si no existe otro vencimiento futuro, no completado, distinto a la referencia...
			List<HistorialNormaSuscrita> historialesFuturos = [.. 
				(await historialNormaSuscritaDao.ObtenerPorNormaSuscrita(historialNormaSuscrita.IdNormaSuscrita, true, transaction))
					.Where(hns => hns.FechaCompletitud == null && hns.Id != historialNormaSuscrita.Id && hns.FechaVencimiento > DateTime.UtcNow)
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
			TipoPeriodicidad tipoPeriodicidad = await tipoPeriodicidadDao.ObtenerPorId(idTipoPeriodicidad, transaction) ?? throw new InvalidOperationException("Tipo periodicidad inválido");
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
						FechaCreacion = DateTime.UtcNow,
						Vigencia = true
					};

					await Crear(nuevoHistorialNormaSuscrita, transaction);
				}
			}
		}
	}
}
