using Amazon.S3.Model;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class CargoBcp(IDateTimeProvider dateTimeProvider, ICargoDao cargoDao) : ICargoBcp {
		public bool EstaVigente(Cargo? cargo) {
			return cargo != null && cargo.Vigencia;
		}

		public bool PerteneceAlNegocio(Cargo cargo, long idNegocio) {
			return cargo.IdNegocio == idNegocio;
		}

		public bool PerteneceAlUsuario(Cargo cargo, string sub) {
			return cargo.Sub == sub;
		}

		public List<Cargo> FiltrarVigentes(List<Cargo> cargos) {
			return [.. cargos.Where(c => EstaVigente(c))];
		}

		public async Task<Cargo?> Obtener(long idCargo, bool filtrarVigente = false, string? filtrarSub = null, long? filtrarIdNegocio = null, bool validarVigencia = false, string? validarSub = null, long? validarIdNegocio = null, NpgsqlTransaction? transaction = null) {
			Cargo? cargo = await cargoDao.Obtener(idCargo, transaction);
			
			// Se aplican todas las validaciones...
			if (validarVigencia && !EstaVigente(cargo)) throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El cargo no existe o no está vigente", "El cargo es inválido.");
			if (cargo != null) {
				if (validarSub != null && !PerteneceAlUsuario(cargo, validarSub)) throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "El cargo no pertenece al usuario", "El cargo es inválido.");
				if (validarIdNegocio != null && !PerteneceAlNegocio(cargo, validarIdNegocio.Value)) throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "El cargo no pertenece al negocio", "El cargo es inválido.");
			}

			// Se aplican los filtros...
			if (filtrarVigente && !EstaVigente(cargo)) return null;
			if (cargo != null) {
				if (filtrarSub != null && !PerteneceAlUsuario(cargo, filtrarSub)) return null;
				if (filtrarIdNegocio != null && !PerteneceAlNegocio(cargo, filtrarIdNegocio.Value)) return null;
			}

			return cargo;
		}

		public async Task<List<Cargo>> ObtenerPorSubYNegocio(string sub, long? idNegocio, bool filtrarVigente = false, NpgsqlTransaction? transaction = null) {
			List<Cargo> cargos = await cargoDao.ObtenerPorSub(sub, idNegocio, null, transaction);
			if (filtrarVigente) cargos = FiltrarVigentes(cargos);
			return cargos;
		}

		public async Task<Cargo> Crear(string sub, string nombre, long idNegocio, NpgsqlTransaction? transaction = null) {
			Cargo nuevo = new() {
				Id = 0,
				Sub = sub,
				Nombre = nombre,
				IdNegocio = idNegocio,
				FechaCreacion = dateTimeProvider.UtcNow,
				FechaEliminacion = null,
				Vigencia = true
			};
			nuevo.Id = await cargoDao.Insertar(nuevo, transaction);
			return nuevo;
		}

		public async Task Actualizar(Cargo cargo, NpgsqlTransaction? transaction = null) {
			await cargoDao.Actualizar(cargo, transaction);
		}

		public async Task Eliminar(Cargo cargo, NpgsqlTransaction? transaction = null) {
			if (cargo.Vigencia) {
				cargo.FechaEliminacion = dateTimeProvider.UtcNow;
				cargo.Vigencia = false;
				await cargoDao.Actualizar(cargo, transaction);
			}
		}
	}
}
