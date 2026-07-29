using Npgsql;
using System.Data.Common;
using System.Transactions;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;

namespace TanatosAPI.UseCases {
	public class CargoUseCase(IDatabaseConnectionHelper connectionHelper, ICargoBcp cargoBcp, INegocioBcp negocioBcp, IEmpleadoBcp empleadoBcp) {
		public async Task<List<Cargo>> ObtenerVigentes(string sub, long idNegocio, NpgsqlTransaction? transaction = null) {
			Negocio negocio = (await negocioBcp.Obtener(idNegocio, validarVigencia: true, validarSub: sub, transaction: transaction))!;
			return await cargoBcp.ObtenerPorSubYNegocio(sub, negocio.Id, filtrarVigente: true, transaction: transaction);
		}

		public async Task<Cargo> Crear(string sub, string nombre, long idNegocio, NpgsqlTransaction? transaction = null) {
			nombre = nombre.Trim();

			Negocio negocio = (await negocioBcp.Obtener(idNegocio, validarVigencia: true, validarSub: sub, transaction: transaction))!;

			List<Cargo> existentes = await cargoBcp.ObtenerPorSubYNegocio(sub, negocio.Id, filtrarVigente: true);
			Cargo? cargoExistente = existentes.FirstOrDefault(c => c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));
			if (cargoExistente != null) {
				return cargoExistente;
			}

			return await cargoBcp.Crear(sub, nombre, negocio.Id);
		}

		public async Task<Cargo> Actualizar(string sub, long idCargo, string nombre, NpgsqlTransaction? transaction = null) {
			nombre = nombre.Trim();

			Cargo existente = (await cargoBcp.Obtener(idCargo, validarVigencia: true, validarSub: sub, transaction: transaction))!;
			Negocio negocio = (await negocioBcp.Obtener(existente.IdNegocio, validarVigencia: true, validarSub: sub, transaction: transaction))!;

			List<Cargo> existentes = await cargoBcp.ObtenerPorSubYNegocio(sub, negocio.Id, transaction: transaction, filtrarVigente: true);
			Cargo? otroMismoNombre = existentes.FirstOrDefault(c => c.Id != idCargo && c.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase));
			if (otroMismoNombre != null) {
				throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "Ya existe un cargo con el mismo nombre");
			}

			if (existente.Nombre != nombre) {
				existente.Nombre = nombre;
				await cargoBcp.Actualizar(existente, transaction);
			}

			return existente;
		}

		public async Task Eliminar(string sub, long idCargo, IDatabaseTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			IDatabaseConnection? connection = null;
			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexionWrapper();
					transaction = await connection.BeginTransactionAsync();
				}

				Cargo? existente = await cargoBcp.Obtener(idCargo, transaction: transaction!.NpgsqlTransaction());
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
