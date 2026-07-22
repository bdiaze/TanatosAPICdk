using Microsoft.AspNetCore.SignalR;
using Npgsql;
using System.Transactions;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
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

        public bool Pertenece(HistorialNormaSuscrita historialNormaSuscrita, long idNormaSuscrita) {
            return historialNormaSuscrita.IdNormaSuscrita == idNormaSuscrita;
        }

        public async Task<HistorialNormaSuscrita?> ObtenerPorId(long idHistorialNormaSuscrita, NpgsqlTransaction? transaction = null) {
			return await historialNormaSuscritaDao.ObtenerPorId(idHistorialNormaSuscrita, transaction);
		}

        public async Task<HistorialNormaSuscrita> ObtenerValidandoVigencia(long idHistorialNormaSuscrita, NpgsqlTransaction? transaction = null) {
			HistorialNormaSuscrita? vencimiento = await ObtenerPorId(idHistorialNormaSuscrita, transaction);
			if (!EstaVigente(vencimiento)) throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El vencimiento no está vigente.");
            return vencimiento!;
        }

		public async Task<HistorialNormaSuscrita> ObtenerValidandoVigenciaYPertenencia(long idHistorialNormaSuscrita, long idNormaSuscrita, NpgsqlTransaction? transaction = null) {
			HistorialNormaSuscrita vencimiento = await ObtenerValidandoVigencia(idHistorialNormaSuscrita, transaction);
            if (!Pertenece(vencimiento, idNormaSuscrita)) throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "El vencimiento no pertenece a la obligación", "El vencimiento no está vigente.");
            return vencimiento!;
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

		public async Task<bool> TieneVencimientoFuturoNoCompletado(long idNormaSuscrita, List<long>? idVencimientoIgnorar = null, NpgsqlTransaction? transaction = null) {
			HashSet<long> idsIgnorar = [.. idVencimientoIgnorar ?? []];
			List<HistorialNormaSuscrita> noCompletados = await ObtenerVigentesPorNormaSuscritaNoCompletadas(idNormaSuscrita, transaction);
			return noCompletados.Any(v => !idsIgnorar.Contains(v.Id) && v.FechaVencimiento > dateTimeProvider.UtcNow);
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

		public async Task<DateTime> Completar(HistorialNormaSuscrita historialNormaSuscrita, NpgsqlTransaction? transaction = null) {
			if (historialNormaSuscrita.FechaCompletitud == null) {
				historialNormaSuscrita.FechaCompletitud = dateTimeProvider.UtcNow;
				await historialNormaSuscritaDao.Actualizar(historialNormaSuscrita, transaction);
			}

			return historialNormaSuscrita.FechaCompletitud.Value;
		}
    }
}
