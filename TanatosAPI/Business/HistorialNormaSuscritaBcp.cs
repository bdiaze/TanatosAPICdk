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

        public bool Pertenece(HistorialNormaSuscrita historialNormaSuscrita, long idNormaSuscrita) {
            return historialNormaSuscrita.IdNormaSuscrita == idNormaSuscrita;
        }

		public List<HistorialNormaSuscrita> FiltrarVigentes(List<HistorialNormaSuscrita> vencimientos) {
			return [.. vencimientos.Where(v => EstaVigente(v))];
		}

		public List<HistorialNormaSuscrita> FiltrarNoCompletadas(List<HistorialNormaSuscrita> vencimientos) {
			return [.. vencimientos.Where(v => !EstaCompletada(v))];
		}

		public List<HistorialNormaSuscrita> FiltrarCompletadas(List<HistorialNormaSuscrita> vencimientos) {
			return [.. vencimientos.Where(v => EstaCompletada(v))];
		}

		public HistorialNormaSuscrita? FiltrarUltimoVencimiento(List<HistorialNormaSuscrita> vencimientos) {
			return vencimientos.OrderByDescending(hns => hns.FechaVencimiento).FirstOrDefault();
		}

		public async Task<HistorialNormaSuscrita?> Obtener(long idHistorialNormaSuscrita, bool validarVigencia = false, long? validarIdNormaSuscrita = null, NpgsqlTransaction? transaction = null) {
			HistorialNormaSuscrita? vencimiento = await historialNormaSuscritaDao.ObtenerPorId(idHistorialNormaSuscrita, transaction);
			// Se aplican todas las validaciones...
			if (validarVigencia && !EstaVigente(vencimiento)) throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El vencimiento no está vigente.");
			if (vencimiento != null) {
				if (validarIdNormaSuscrita != null && !Pertenece(vencimiento, validarIdNormaSuscrita.Value)) throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "El vencimiento no pertenece a la obligación", "El vencimiento no está vigente.");
			}

			return vencimiento;
		}

		public async Task<List<HistorialNormaSuscrita>> ObtenerPorNormaSuscrita(long idNormaSuscrita, bool filtrarVigente = false, bool filtrarNoCompletadas = false, bool filtrarCompletadas = false, NpgsqlTransaction? transaction = null) {
			if (filtrarCompletadas && filtrarNoCompletadas) throw new InvalidOperationException("No se puede filtrar por completadas y no completadas al mismo tiempo");
			
			List<HistorialNormaSuscrita> vencimientos = await historialNormaSuscritaDao.ObtenerPorNormaSuscrita(idNormaSuscrita, null, transaction);
			if (filtrarVigente) vencimientos = FiltrarVigentes(vencimientos);
			if (filtrarNoCompletadas) vencimientos = FiltrarNoCompletadas(vencimientos);
			if (filtrarCompletadas) vencimientos = FiltrarCompletadas(vencimientos);
			return vencimientos;
		}

		public async Task<DateTime> ObtenerProximoVencimiento(long idNormaSuscrita, NpgsqlTransaction? transaction = null) {
			HistorialNormaSuscrita proximoVencimiento = (await historialNormaSuscritaDao.ObtenerPorNormaSuscritaYFechaCompletitud(idNormaSuscrita, null, true, transaction))
				.OrderByDescending(v => v.FechaVencimiento)
				.FirstOrDefault() ?? throw new InvalidOperationException("La obligación no cuenta con un vencimiento no completado.");
			return proximoVencimiento.FechaVencimiento;
		}

		public async Task<bool> TieneVencimientoFuturoNoCompletado(long idNormaSuscrita, List<long>? idVencimientoIgnorar = null, NpgsqlTransaction? transaction = null) {
			HashSet<long> idsIgnorar = [.. idVencimientoIgnorar ?? []];
			List<HistorialNormaSuscrita> noCompletados = await ObtenerPorNormaSuscrita(idNormaSuscrita, filtrarVigente: true, filtrarNoCompletadas: true, transaction: transaction);
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
				if (historialNormaSuscrita.FechaCompletitud != null) throw new ErrorValidacion(TipoErrorValidacion.EstadoNoValido, "No se puede eliminar un vencimiento ya completado.");

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
