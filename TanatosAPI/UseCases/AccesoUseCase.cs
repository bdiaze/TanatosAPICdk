using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.UseCases;

namespace TanatosAPI.UseCases {
	public class AccesoUseCase(IDatabaseConnectionHelper connectionHelper, IAccesoBcp accesoBcp, IRecursoBcp recursoBcp, IPermisoBcp permisoBcp) : IAccesoUseCase {
		public async Task<(Acceso, string CodigoAcceso)> HabilitarAcceso(List<RecursoSolicitado> recursosSolicitados, TimeSpan? duracion = null, IDatabaseTransaction? transaction = null) {
			if (recursosSolicitados.Count == 0) throw new InvalidOperationException("No se puede solicitar un acceso sin definir los recursos asociados.");

			bool ownsTransaction = transaction == null;
			IDatabaseConnection? connection = null;
			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexionWrapper();
					transaction = await connection.BeginTransactionAsync();
				}

				(Acceso acceso, string codigoAcceso) = await accesoBcp.Crear(duracion, transaction!.NpgsqlTransaction());
				acceso.Permisos = [];
				foreach (RecursoSolicitado recursoSolicitado in recursosSolicitados) {
					Recurso recurso = await recursoBcp.Crear(recursoSolicitado.Tipo, recursoSolicitado.IdInterno, transaction!.NpgsqlTransaction());
					foreach (string accion in recursoSolicitado.Acciones) {
						Permiso permiso = await permisoBcp.Crear(acceso.Id, recurso.Id, accion, transaction!.NpgsqlTransaction());
						permiso.Recurso = recurso;
						acceso.Permisos.Add(permiso);
					}
				}

				if (ownsTransaction) {
					await transaction!.CommitAsync();
				}

				return (acceso, codigoAcceso);
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

		public async Task<Acceso> ValidarAcceso(string codigoAcceso, string tipoRecurso, string idRecurso, string accion, NpgsqlTransaction? transaction = null) {
			Acceso acceso = (await accesoBcp.ObtenerPorCodigo(codigoAcceso, validarVigencia: true, validarExpiracion: true, transaction: transaction))!;
			List<Permiso> permisos = [.. 
				(await permisoBcp.ObtenerPorIdAcceso(acceso.Id, filtrarVigentes: true, transaction: transaction))
					.Where(p => p.Accion == accion)
			];
			List<Recurso> recursos = [.. 
				(await recursoBcp.ObtenerVarios([.. permisos.Select(p => p.IdRecurso)], filtrarVigentes: true, transaction: transaction)).
					Where(r => r.Tipo == tipoRecurso && r.IdInterno == idRecurso)
			];

			if (recursos.Count == 0) throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "El código de acceso no tiene permiso sobre el recurso", "El código de acceso es inválido.");
			return acceso;
		}
	}

	public class RecursoSolicitado {
		public required string Tipo { get; set; }
		public required string IdInterno { get; set; }
		public required HashSet<string> Acciones { get; set; }
	}
}
