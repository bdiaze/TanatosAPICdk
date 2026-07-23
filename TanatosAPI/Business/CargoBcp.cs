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

		public async Task<Cargo?> Obtener(long idCargo, NpgsqlTransaction? transaction = null) {
			return await cargoDao.Obtener(idCargo, transaction);
		}

		public async Task<Cargo?> ObtenerSoloVigente(long idCargo, NpgsqlTransaction? transaction = null) {
			Cargo? existente = await Obtener(idCargo, transaction);
			if (EstaVigente(existente)) return existente;
			return null;
		}

		public async Task<Cargo> ObtenerValidandoVigencia(long idCargo, NpgsqlTransaction? transaction = null) {
            Cargo? existente = await Obtener(idCargo, transaction);
            if (!EstaVigente(existente)) throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El cargo no existe o no está vigente", "El cargo es inválido.");
			return existente!;
        }

		public async Task<Cargo> ObtenerValidandoVigenciaYPertenencia(long idCargo, string sub, NpgsqlTransaction? transaction = null) {
			Cargo existente = await ObtenerValidandoVigencia(idCargo, transaction);
            if (!PerteneceAlUsuario(existente, sub)) throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "El cargo no pertenece al usuario", "El cargo es inválido.");
			return existente!;
		}

		public async Task<Cargo> ObtenerValidandoVigenciaPertenenciaNegocio(long idCargo, long idNegocio, string sub, NpgsqlTransaction? transaction = null) {
			Cargo existente = await ObtenerValidandoVigenciaYPertenencia(idCargo, sub, transaction);
			if (!PerteneceAlNegocio(existente, idNegocio)) throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "El cargo no pertenece al negocio", "El cargo es inválido.");
            return existente!;
        }

		public async Task<List<Cargo>> ObtenerVigentes(string sub, long? idNegocio, NpgsqlTransaction? transaction = null) {
			return await cargoDao.ObtenerPorSub(sub, idNegocio, true, transaction);
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
