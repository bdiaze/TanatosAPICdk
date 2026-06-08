using Npgsql;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;

namespace TanatosAPI.UseCases {
	public class CargoUseCase(DatabaseConnectionHelper connectionHelper, CargoBcp cargoBcp, NegocioBcp negocioBcp, EmpleadoBcp empleadoBcp) {
		public async Task<List<Cargo>> ObtenerVigentes(string sub, long idNegocio) {
			Negocio? negocio = await negocioBcp.ObtenerPorId(idNegocio);
			if (!negocioBcp.EstaVigente(negocio)) {
				throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El negocio no existe o no está vigente", "El negocio es inválido.");
			}

			if (!negocioBcp.PerteneceAlUsuario(negocio!, sub)) {
				throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "El negocio no pertenece al usuario", "El negocio es inválido.");
			}

			return await cargoBcp.ObtenerVigentes(sub, idNegocio);
		}

		public async Task<Cargo> RegistrarCargo(string sub, string nombre, long idNegocio) {
			nombre = nombre.Trim();

			Negocio? negocio = await negocioBcp.ObtenerPorId(idNegocio);
			if (!negocioBcp.EstaVigente(negocio)) {
				throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El negocio no existe o no está vigente", "El negocio es inválido.");
			}

			if (!negocioBcp.PerteneceAlUsuario(negocio!, sub)) {
				throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "El negocio no pertenece al usuario", "El negocio es inválido.");
			}

			List<Cargo> existentes = await cargoBcp.ObtenerVigentes(sub, idNegocio);
			Cargo? cargoExistente = existentes.FirstOrDefault(c => c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));
			if (cargoExistente != null) {
				return cargoExistente;
			}

			return await cargoBcp.Insertar(sub, nombre, idNegocio);
		}

		public async Task<Cargo> ActualizarCargo(string sub, long idCargo, string nombre) {
			nombre = nombre.Trim();

			Cargo? existente = await cargoBcp.ObtenerPorId(idCargo);
			if (!cargoBcp.EstaVigente(existente)) {
				throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El cargo no existe o no está vigente", "El cargo es inválido.");
			}

			if (!cargoBcp.PerteneceAlUsuario(existente!, sub)) {
				throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "El cargo no pertenece al usuario", "El cargo es inválido.");
			}

			Negocio? negocio = await negocioBcp.ObtenerPorId(existente!.IdNegocio);
			if (!negocioBcp.EstaVigente(negocio)) {
				throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "El negocio no existe o no está vigente", "El cargo es inválido.");
			}

			if (!negocioBcp.PerteneceAlUsuario(negocio!, sub)) {
				throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "El negocio no pertenece al usuario", "El cargo es inválido.");
			}

			List<Cargo> existentes = await cargoBcp.ObtenerVigentes(sub, existente!.IdNegocio);
			Cargo? otroMismoNombre = existentes.FirstOrDefault(c => c.Id != idCargo && c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));
			if (otroMismoNombre != null) {
				throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "Ya existe un cargo con el mismo nombre");
			}

			if (existente.Nombre != nombre) {
				existente.Nombre = nombre;
				await cargoBcp.Modificar(existente);
			}

			return existente;
		}

		public async Task EliminarCargo(string sub, long idCargo, NpgsqlTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			NpgsqlConnection? connection = null;
			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexion();
					transaction = await connection.BeginTransactionAsync();
				}

				Cargo? existente = await cargoBcp.ObtenerPorId(idCargo, transaction);
				if (existente != null && !cargoBcp.PerteneceAlUsuario(existente, sub)) {
					throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "El cargo no pertenece al usuario", "El cargo es inválido.");
				}

				if (!cargoBcp.EstaVigente(existente)) {
					return;
				}

				await empleadoBcp.DesasociarCargo(sub, existente!.IdNegocio, existente!.Id, transaction);

				await cargoBcp.Eliminar(existente!, transaction);

				if (ownsTransaction) {
					await transaction!.CommitAsync();
				}
			} catch {
				if (ownsTransaction && transaction != null) {
					await transaction.RollbackAsync();
				}
				throw;
			} finally {
				if (ownsTransaction) {
					if (transaction != null) await transaction.DisposeAsync();
					if (connection != null) await connection.DisposeAsync();
				}
			}
		}
	}
}
