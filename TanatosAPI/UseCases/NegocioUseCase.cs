using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Kairos;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;

namespace TanatosAPI.UseCases {
	public class NegocioUseCase(IDatabaseConnectionHelper connectionHelper, IDateTimeProvider dateTimeProvider, NormaSuscritaUseCase normaSuscritaUseCase, INegocioBcp negocioBcp, ISuscripcionBcp suscripcionBcp, ITipoActividadBcp tipoActividadBcp, ITipoRubroBcp tipoRubroBcp) {
		public async Task IncluirActivididad(Negocio negocio, NpgsqlTransaction? transaction = null) {
			await IncluirActivididad([negocio], transaction);
		}

		public async Task IncluirActivididad(List<Negocio> negocios, NpgsqlTransaction? transaction = null) {
			if (negocios.Any(n => n.IdTipoActividad != null)) {
				Dictionary<long, TipoActividad> actividades = (await tipoActividadBcp.ObtenerTodos(filtrarVigentes: true, transaction: transaction)).ToDictionary(a => a.Id, a => a);
				Dictionary<long, TipoRubro> rubros = (await tipoRubroBcp.ObtenerTodos(filtrarVigentes: true, transaction: transaction)).ToDictionary(r => r.Id, r => r);				
				
				foreach (long idActividad in actividades.Keys.ToList()) {
					TipoActividad actividad = actividades[idActividad];
					if (rubros.TryGetValue(actividad.IdTipoRubro, out TipoRubro? rubro) && rubro != null) {
						actividad.TipoRubro = rubro;
					} else {
						actividades.Remove(actividad.Id);
					}
				}
			}
		}
		
		public async Task<Negocio?> Obtener(long id, bool validarVigencia = false, string? validarSub = null, bool validarSegunPlan = false, bool incluirActividad = false, NpgsqlTransaction? transaction = null) {
			Negocio? negocio = await negocioBcp.Obtener(id, validarVigencia: validarVigencia, validarSub: validarSub, transaction: transaction);
			if (negocio != null) {
				if (validarSegunPlan) {
					Negocio? primerNegocio = await negocioBcp.ObtenerPrimerNegocio(negocio.Sub, transaction);
					if (primerNegocio?.Id != negocio.Id && !await suscripcionBcp.ConsultaTienePlanEmpresa(negocio.Sub, transaction)) {
						throw new ErrorValidacion(TipoErrorValidacion.RestringidoPorPlan, "El usuario no tiene Plan Empresa y no es su primer negocio", "Tu plan no permite actualizar la información de este negocio.");
					}
				}

				if (incluirActividad) await IncluirActivididad(negocio, transaction);
			}
			return negocio;
		}
		
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

		public async Task<(List<SalKairosIngresarProceso> procesosProgramados, List<NormaSuscritaProcesoNotificacion> procesosDesprogramados)> EliminarNegocio(Negocio negocio, IDatabaseTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			IDatabaseConnection? connection = null;

			List<SalKairosIngresarProceso> procesosProgramados = [];
			List<NormaSuscritaProcesoNotificacion> procesosDesprogramados = [];
			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexionWrapper();
					transaction = await connection.BeginTransactionAsync();
				}

				if (negocio.Vigencia) {
					negocio.FechaEliminacion = dateTimeProvider.UtcNow;
					negocio.Vigencia = false;
					await negocioBcp.Actualizar(negocio, transaction!.NpgsqlTransaction());

					List<NormaSuscrita> normasSuscritas = await normaSuscritaUseCase.ObtenerPorSubYNegocio(negocio.Sub, negocio.Id, filtrarVigentes: true, transaction: transaction!.NpgsqlTransaction());
					foreach (NormaSuscrita normaSuscrita in normasSuscritas) {
						(List<SalKairosIngresarProceso> programadosParciales, List<NormaSuscritaProcesoNotificacion> desprogramadosParciales) = await normaSuscritaUseCase.EliminarNormaSuscrita(normaSuscrita, transaction);
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

		public async Task<Negocio> ActualizarMisionVisionValores(string sub, long idNegocio, string? mision, string? vision, string? valores, IDatabaseTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			IDatabaseConnection? connection = null;
			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexionWrapper();
					transaction = await connection.BeginTransactionAsync();
				}

				mision = string.IsNullOrWhiteSpace(mision) ? null : mision?.Trim();
				vision = string.IsNullOrWhiteSpace(vision) ? null : vision?.Trim();
				valores = string.IsNullOrWhiteSpace(valores) ? null : valores?.Trim();

				Negocio negocio = (await Obtener(idNegocio, validarVigencia: true, validarSub: sub, validarSegunPlan: true, incluirActividad: true, transaction: transaction!.NpgsqlTransaction()))!;

				if (negocio.Mision != mision || negocio.Vision != vision || negocio.Valores != valores) {
					negocio.Mision = mision;
					negocio.Vision = vision;
					negocio.Valores = valores;

					await negocioBcp.Actualizar(negocio, transaction!.NpgsqlTransaction());
				}

				if (ownsTransaction) {
					await transaction!.CommitAsync();
				}

				return negocio;
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
