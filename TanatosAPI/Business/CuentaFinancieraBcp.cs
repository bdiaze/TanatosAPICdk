using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;

namespace TanatosAPI.Business {
	public class CuentaFinancieraBcp(IDateTimeProvider dateTimeProvider, ICuentaFinancieraDao cuentaFinancieraDao) : ICuentaFinancieraBcp {
		public bool EstaVigente(CuentaFinanciera? item) {
			return item != null && item.Vigencia;
		}

		public bool Pertenece(CuentaFinanciera item, string sub) {
			return item.Sub == sub;
		}

		public bool PerteneceNegocio(CuentaFinanciera item, long idNegocio) {
			return item.IdNegocio == idNegocio;
		}

		public List<CuentaFinanciera> FiltrarVigentes(List<CuentaFinanciera> items) {
			return [.. items.Where(c => EstaVigente(c))];
		}

		public async Task<CuentaFinanciera?> Obtener(long id, bool validarVigencia = false, string? validarSub = null, long? validarIdNegocio = null, NpgsqlTransaction? transaction = null) {
			CuentaFinanciera? item = await cuentaFinancieraDao.Obtener(id, transaction);
			if (validarVigencia && !EstaVigente(item)) throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "La cuenta financiera no existe o no está vigente", "La cuenta es inválida.");
			if (item != null) {
				if (validarSub != null && !Pertenece(item, validarSub)) throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "La cuenta financiera no pertenece al usuario", "La cuenta es inválida.");
				if (validarIdNegocio != null && !PerteneceNegocio(item, validarIdNegocio.Value)) throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "La cuenta financiera no pertenece al negocio", "La cuenta es inválida.");
			}
			return item;
		}

		public async Task<List<CuentaFinanciera>> ObtenerPorSub(string sub, long? idNegocio = null, bool filtrarVigentes = false, NpgsqlTransaction? transaction = null) {
			List<CuentaFinanciera> items = await cuentaFinancieraDao.ObtenerPorSub(sub, idNegocio, transaction);
			if (filtrarVigentes) items = FiltrarVigentes(items);
			return items;
		}

		public async Task<CuentaFinanciera> Crear(string sub, long idNegocio, string nombre, long idTipoCuentaFinanciera, decimal saldoInicial, DateTime fechaSaldoInicialUtc, NpgsqlTransaction? transaction = null) {
			CuentaFinanciera nuevo = new() { 
				Id = 0,
				Sub = sub,
				IdNegocio = idNegocio,
				Nombre = nombre,
				IdTipoCuentaFinanciera = idTipoCuentaFinanciera,
				SaldoInicial = saldoInicial,
				FechaSaldoInicial = fechaSaldoInicialUtc,
				SaldoActual = saldoInicial,
				FechaCreacion = dateTimeProvider.UtcNow,
				FechaEliminacion = null,
				Vigencia = true
			};
			nuevo.Id = await cuentaFinancieraDao.Insertar(nuevo, transaction);
			return nuevo;
		}

		public async Task Actualizar(CuentaFinanciera item, NpgsqlTransaction? transaction = null) {
			await cuentaFinancieraDao.Actualizar(item, transaction);
		}

		public async Task Eliminar(CuentaFinanciera item, NpgsqlTransaction? transaction = null) {
			if (EstaVigente(item)) {
				item.FechaEliminacion = dateTimeProvider.UtcNow;
				item.Vigencia = false;
				await cuentaFinancieraDao.Actualizar(item, transaction);
			}
		}
	}
}
