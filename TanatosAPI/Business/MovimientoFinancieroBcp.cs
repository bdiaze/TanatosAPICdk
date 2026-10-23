using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class MovimientoFinancieroBcp(IDateTimeProvider dateTimeProvider, IMovimientoFinancieroDao movimientoFinancieroDao) : IMovimientoFinancieroBcp {
		public bool EstaVigente(MovimientoFinanciero? item) {
			return item != null && item.Vigencia;
		}

		public bool PerteneceCuenta(MovimientoFinanciero item, long idCuentaFinanciera) {
			return item.IdCuentaFinanciera == idCuentaFinanciera;
		}

		public List<MovimientoFinanciero> FiltrarVigentes(List<MovimientoFinanciero> items) {
			return [.. items.Where(c => EstaVigente(c))];
		}

		public async Task<MovimientoFinanciero?> Obtener(long id, bool filtrarVigente = false, bool validarVigencia = false, long? validarIdCuentaFinanciera = null, NpgsqlTransaction? transaction = null) {
			MovimientoFinanciero? item = await movimientoFinancieroDao.Obtener(id, transaction);
			if (validarVigencia && !EstaVigente(item)) throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El movimiento financiero no existe o no está vigente", "El movimiento es inválido.");
			
			if (item != null) {
				if (validarIdCuentaFinanciera != null && !PerteneceCuenta(item, validarIdCuentaFinanciera.Value)) throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "El movimiento no pertenece a la cuenta financiera", "El movimiento es inválido.");
			}

			// Se aplican los filtros...
			if (filtrarVigente && !EstaVigente(item)) return null;

			return item;
		}

		public async Task<List<MovimientoFinanciero>> ObtenerConPaginacion(long idCuentaFinanciera, DateTime fechaDesde, DateTime fechaHasta, bool? vigencia = true, DateTime? primeraFecha = null, long? primerId = null, int cantidad = 50, NpgsqlTransaction? transaction = null) {
			return await movimientoFinancieroDao.ObtenerConPaginacion(idCuentaFinanciera, fechaDesde, fechaHasta, vigencia, primeraFecha, primerId, cantidad, transaction);
		}

		public async Task<List<MovimientoFinanciero>> ObtenerPrevios(long idCuentaFinanciera, DateTime fechaReferencia, long? idReferencia = null, bool? vigencia = true, int cantidad = 1, NpgsqlTransaction? transaction = null) {
			return await movimientoFinancieroDao.ObtenerPrevios(idCuentaFinanciera, fechaReferencia, idReferencia, vigencia, cantidad, transaction);
		}

		public async Task<List<MovimientoFinanciero>> ObtenerPosteriores(long idCuentaFinanciera, DateTime fechaReferencia, long idReferencia, bool? vigencia = true, bool incluirReferencia = false, NpgsqlTransaction? transaction = null) {
			return await movimientoFinancieroDao.ObtenerPosteriores(idCuentaFinanciera, fechaReferencia, idReferencia, vigencia, incluirReferencia, transaction);
		}

		public async Task<MovimientoFinanciero> Crear(long idCuentaFinanciera, long? idCategoriaFinanciera, short tipoMovimiento, decimal monto, decimal saldo, DateTime fechaMovimientoUtc, string? descripcion, NpgsqlTransaction? transaction = null) {
			MovimientoFinanciero nuevo = new() {
				Id = 0,
				IdCuentaFinanciera = idCuentaFinanciera,
				IdCategoriaFinanciera = idCategoriaFinanciera,
				TipoMovimiento = tipoMovimiento,
				Monto = monto,
				Saldo = saldo,
				FechaMovimiento = fechaMovimientoUtc,
				Descripcion = descripcion,
				FechaCreacion = dateTimeProvider.UtcNow,
				FechaEliminacion = null,
				Vigencia = true
			};
			nuevo.Id = await movimientoFinancieroDao.Insertar(nuevo, transaction);
			return nuevo;
		}

		public async Task Actualizar(MovimientoFinanciero item, NpgsqlTransaction? transaction = null) {
			await movimientoFinancieroDao.Actualizar(item, transaction);
		}

		public async Task Eliminar(MovimientoFinanciero item, NpgsqlTransaction? transaction = null) {
			if (EstaVigente(item)) {
				item.FechaEliminacion = dateTimeProvider.UtcNow;
				item.Vigencia = false;
				await movimientoFinancieroDao.Actualizar(item, transaction);
			}
		}
	}
}
