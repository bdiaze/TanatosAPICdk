using Amazon.S3.Model;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class CargoBcp(IDateTimeProvider dateTimeProvider, ICargoDao cargoDao) {
		public bool EstaVigente(Cargo? cargo) {
			return cargo != null && cargo.Vigencia;
		}

		public bool PerteneceAlUsuario(Cargo cargo, string sub) {
			return cargo.Sub == sub;
		}

		public async Task<Cargo?> ObtenerPorId(long idCargo, NpgsqlTransaction? transaction = null) {
			return await cargoDao.Obtener(idCargo, transaction);
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
