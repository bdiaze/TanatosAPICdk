using Microsoft.AspNetCore.SignalR;
using Npgsql;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Interfaces.UseCases;
using TanatosAPI.Repositories;

namespace TanatosAPI.UseCases {
	public class NegocioUseCase(IDatabaseConnectionHelper connectionHelper, IDateTimeProvider dateTimeProvider, INegocioBcp negocioBcp, ISuscripcionBcp suscripcionBcp, INegocioDao negocioDao, INormaSuscritaDao normaSuscritaDao, NormaSuscritaUseCase normaSuscritaUseCase) {
		public async Task<bool> NegocioAccesible(string sub, long idNegocio, NpgsqlTransaction? transaction = null) {
			// Se valida que el negocio sea del usuario...
			List<Negocio> negocios = await negocioBcp.ObtenerPorSub(sub, filtrarVigentes: true, transaction: transaction);
			Negocio? negocioSeleccionado = negocios.FirstOrDefault(n => n.Id == idNegocio);
			if (negocioSeleccionado == null) return false;

			// Se valida si el usuario tiene plan Empresa...
			bool tienePlanEmpresa = await suscripcionBcp.ConsultaTienePlanEmpresa(sub, transaction);
			if (tienePlanEmpresa) return true;

			// Dado que no tiene plan Empresa, se valida si el negocio corresponde al primer negocio creado por el usuario...
			Negocio primerNegocio = negocios.OrderBy(n => n.FechaCreacion).First();
			if (primerNegocio.Id != negocioSeleccionado.Id) return false;
			else return true;
		}

		public async Task<(List<ProcesoNotificacion> procesosProgramados, List<ProcesoNotificacion> procesosDesprogramados)> EliminarNegocio(Negocio negocio, IDatabaseTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			IDatabaseConnection? connection = null;

			List<ProcesoNotificacion> procesosProgramados = [];
			List<ProcesoNotificacion> procesosDesprogramados = [];
			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexionWrapper();
					transaction = await connection.BeginTransactionAsync();
				}

				if (negocio.Vigencia) {
					negocio.FechaEliminacion = dateTimeProvider.UtcNow;
					negocio.Vigencia = false;
					await negocioDao.Actualizar(negocio, transaction!.NpgsqlTransaction());

					List<NormaSuscrita> normasSuscritas = await normaSuscritaDao.ObtenerPorSub(negocio.Sub, negocio.Id, true, transaction!.NpgsqlTransaction());
					foreach (NormaSuscrita normaSuscrita in normasSuscritas) {
						(List<ProcesoNotificacion> programadosParciales, List<ProcesoNotificacion> desprogramadosParciales) = await normaSuscritaUseCase.EliminarNormaSuscrita(normaSuscrita, transaction);
						procesosProgramados.AddRange(programadosParciales);
						procesosDesprogramados.AddRange(desprogramadosParciales);
					}
				}

				if (ownsTransaction) {
					await transaction!.CommitAsync();
				}

				return (procesosProgramados, procesosDesprogramados);
			} catch {
				if (ownsTransaction && transaction != null) {
					await transaction.RollbackAsync();
					await normaSuscritaUseCase.ReversarProcesosProgramadosDesprogramados(procesosProgramados, procesosDesprogramados);
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
