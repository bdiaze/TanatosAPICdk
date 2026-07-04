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

		public bool PerteneceAlUsuario(Cargo cargo, string sub) {
			return cargo.Sub == sub;
		}

		public async Task<Cargo?> ObtenerPorId(long idCargo, NpgsqlTransaction? transaction = null) {
			return await cargoDao.Obtener(idCargo, transaction);
		}

		public async Task<Cargo> ObtenerPorIdValidandoVigenciaYPertenencia(long idCargo, string sub, NpgsqlTransaction? transaction = null) {
			Cargo? existente = await ObtenerPorId(idCargo, transaction);
			if (!EstaVigente(existente)) {
				throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El cargo no existe o no está vigente", "El cargo es inválido.");
			}

			if (!PerteneceAlUsuario(existente!, sub)) {
				throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "El cargo no pertenece al usuario", "El cargo es inválido.");
			}
			return existente!;
		}

		public async Task<List<Cargo>> ObtenerVigentes(string sub, long? idNegocio) {
			return await cargoDao.ObtenerPorSub(sub, idNegocio, true);
		}

		public async Task<Cargo> Insertar(string sub, string nombre, long idNegocio) {
			Cargo nuevo = new() {
				Id = 0,
				Sub = sub,
				Nombre = nombre,
				IdNegocio = idNegocio,
				FechaCreacion = dateTimeProvider.UtcNow,
				FechaEliminacion = null,
				Vigencia = true
			};
			nuevo.Id = await cargoDao.Insertar(nuevo);
			return nuevo;
		}

		public async Task Modificar(Cargo cargo) {
			await cargoDao.Actualizar(cargo);
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
