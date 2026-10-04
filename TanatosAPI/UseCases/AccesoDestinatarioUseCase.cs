using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.UseCases;

namespace TanatosAPI.UseCases {
	public class AccesoDestinatarioUseCase(IDatabaseConnectionHelper connectionHelper, IAccesoUseCase accesoUseCase, IAccesoDestinatarioBcp accesoDestinatarioBcp, IDestinatarioNotificacionBcp destinatarioNotificacionBcp) : IAccesoDestinatarioUseCase {
		public async Task<(AccesoDestinatario, string CodigoAcceso)> HabilitarAccesoDestinatario(long idDestinatario, List<RecursoSolicitado> recursosSolicitados, TimeSpan? duracion = null, IDatabaseTransaction? transaction = null) {
			if (recursosSolicitados.Count == 0) throw new InvalidOperationException("No se puede solicitar un acceso a destinatario sin definir los recursos asociados.");

			bool ownsTransaction = transaction == null;
			IDatabaseConnection? connection = null;
			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexionWrapper();
					transaction = await connection.BeginTransactionAsync();
				}

				(Acceso acceso, string codigoAcceso) = await accesoUseCase.HabilitarAcceso(recursosSolicitados, duracion, transaction);
				AccesoDestinatario accesoDestinatario = await accesoDestinatarioBcp.Crear(acceso.Id, idDestinatario, transaction!.NpgsqlTransaction());
				accesoDestinatario.Acceso = acceso;

				if (ownsTransaction) {
					await transaction!.CommitAsync();
				}

				return (accesoDestinatario, codigoAcceso);
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

		public async Task<DestinatarioNotificacion> ValidarAccesoDestinatario(string codigoAcceso, string tipoRecurso, string idRecurso, string accion, NpgsqlTransaction? transaction = null) {
			Acceso acceso = await accesoUseCase.ValidarAcceso(codigoAcceso, tipoRecurso, idRecurso, accion, transaction);
			
			List<AccesoDestinatario> accesoDestinatarios = await accesoDestinatarioBcp.ObtenerPorIdAcceso(acceso.Id, filtrarVigentes: true, transaction: transaction);
			if (accesoDestinatarios.Count != 1) throw new ErrorValidacion(TipoErrorValidacion.EstadoNoValido, "El código de acceso no es un acceso de destinatario", "El código de acceso es inválido.");
			
			AccesoDestinatario accesoDestinatario = accesoDestinatarios.First();
			DestinatarioNotificacion? destinatarioNotificacion = await destinatarioNotificacionBcp.Obtener(accesoDestinatario.IdDestinatarioNotificacion, filtrarVigente: true, filtrarValidado: true, transaction: transaction);
			if (destinatarioNotificacion == null) throw new ErrorValidacion(TipoErrorValidacion.EstadoNoValido, "El destinatario asociado al acceso de destinatario no está vigente o no está validado", "El código de acceso es inválido.");

			return destinatarioNotificacion;
		}
	}
}
