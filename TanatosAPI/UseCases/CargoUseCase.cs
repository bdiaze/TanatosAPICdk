using Npgsql;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;

namespace TanatosAPI.UseCases {
	public class CargoUseCase(IDatabaseConnectionHelper connectionHelper, ICargoBcp cargoBcp, INegocioBcp negocioBcp, IEmpleadoBcp empleadoBcp) {
		public async Task<List<Cargo>> ObtenerVigentes(string sub, long idNegocio) {
			Negocio negocio = await negocioBcp.ObtenerPorIdValidandoVigenciaYPertenencia(idNegocio, sub);
			return await cargoBcp.ObtenerVigentes(sub, negocio.Id);
		}

		public async Task<Cargo> RegistrarCargo(string sub, string nombre, long idNegocio) {
			nombre = nombre.Trim();

			Negocio negocio = await negocioBcp.ObtenerPorIdValidandoVigenciaYPertenencia(idNegocio, sub);

			List<Cargo> existentes = await cargoBcp.ObtenerVigentes(sub, negocio.Id);
			Cargo? cargoExistente = existentes.FirstOrDefault(c => c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));
			if (cargoExistente != null) {
				return cargoExistente;
			}

			return await cargoBcp.Insertar(sub, nombre, negocio.Id);
		}

		public async Task<Cargo> ActualizarCargo(string sub, long idCargo, string nombre) {
			nombre = nombre.Trim();

			Cargo existente = await cargoBcp.ObtenerPorIdValidandoVigenciaYPertenencia(idCargo, sub);
			Negocio? negocio = await negocioBcp.ObtenerPorIdValidandoVigenciaYPertenencia(existente!.IdNegocio, sub);

			List<Cargo> existentes = await cargoBcp.ObtenerVigentes(sub, negocio.Id);
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

		public async Task EliminarCargo(string sub, long idCargo, IDatabaseTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			IDatabaseConnection? connection = null;
			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexionWrapper();
					transaction = await connection.BeginTransactionAsync();
				}

				Cargo? existente = await cargoBcp.ObtenerPorId(idCargo, transaction!.NpgsqlTransaction());
				if (existente != null && !cargoBcp.PerteneceAlUsuario(existente, sub)) {
					throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "El cargo no pertenece al usuario", "El cargo es inválido.");
				}

				if (!cargoBcp.EstaVigente(existente)) {
					return;
				}

				await empleadoBcp.DesasociarCargo(sub, existente!.IdNegocio, existente!.Id, transaction!.NpgsqlTransaction());

				await cargoBcp.Eliminar(existente!, transaction!.NpgsqlTransaction());

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
