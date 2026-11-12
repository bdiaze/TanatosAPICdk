using Microsoft.AspNetCore.SignalR;
using System.Transactions;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;

namespace TanatosAPI.UseCases {
	public class PerfilUseCase(IDatabaseConnectionHelper connectionHelper, IUsuarioBcp usuarioBcp, SuscripcionUseCase suscripcionUseCase, ICognitoHelper cognitoHelper) {
		public async Task<Usuario> ConfiguracionInicial(string userName, IDatabaseTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			IDatabaseConnection? connection = null;

			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexionWrapper();
					transaction = await connection.BeginTransactionAsync();
				}

				Usuario usuario = await usuarioBcp.CargarDesdeCognitoSiNoExiste(userName, transaction!.NpgsqlTransaction());
				_ = await suscripcionUseCase.SuscribirseAPlanesGratuitos(usuario.Sub, transaction);

				if (ownsTransaction) {
					await transaction!.CommitAsync();
				}

				return usuario;
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

		public async Task<Usuario> CrearUsuario(string email, string nombre, string apellido, IDatabaseTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			IDatabaseConnection? connection = null;

			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexionWrapper();
					transaction = await connection.BeginTransactionAsync();
				}

				string userName = await cognitoHelper.CrearUsuario(email, nombre, apellido);
				Usuario usuario = await ConfiguracionInicial(userName, transaction);

				if (ownsTransaction) {
					await transaction!.CommitAsync();
				}

				return usuario;
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
