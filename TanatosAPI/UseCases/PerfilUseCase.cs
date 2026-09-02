using Microsoft.AspNetCore.SignalR;
using System.Transactions;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;

namespace TanatosAPI.UseCases {
	public class PerfilUseCase(IDatabaseConnectionHelper connectionHelper, IUsuarioBcp usuarioBcp, SuscripcionUseCase suscripcionUseCase) {
		public async Task ConfiguracionInicial(string userName, IDatabaseTransaction? transaction = null) {
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
