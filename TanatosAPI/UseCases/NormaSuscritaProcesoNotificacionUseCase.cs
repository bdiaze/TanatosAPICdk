using Npgsql;
using System.Data.Common;
using System.Text.Json;
using System.Transactions;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Kairos;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.UseCases;

namespace TanatosAPI.UseCases {
	public class NormaSuscritaProcesoNotificacionUseCase(IDatabaseConnectionHelper connectionHelper, IVariableEntornoHelper variableEntornoHelper, IKairosHelper kairosHelper, INormaSuscritaProcesoNotificacionBcp normaSuscritaProcesoNotificacionBcp, IProcesoAutomaticoBcp procesoAutomaticoBcp) : INormaSuscritaProcesoNotificacionUseCase {
		private readonly long ID_TIPO_PROCESO_NOTIFICACION = 1;
		private readonly string APP_NAME = variableEntornoHelper.Obtener("APP_NAME");
		private readonly string NOTIFICACIONES_LAMBDA_ARN = variableEntornoHelper.Obtener("NOTIFICACIONES_LAMBDA_ARN");
		private readonly string NOTIFICACIONES_EJECUCION_ROLE_ARN = variableEntornoHelper.Obtener("NOTIFICACIONES_EJECUCION_ROLE_ARN");

		public List<NormaSuscritaProcesoNotificacion> ExtraerCronsAEliminar(List<NormaSuscritaProcesoNotificacion> procesosNotificacion, List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> cronsDeseados) {
			HashSet<(string Cron, long? IdUnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> deseados = [.. cronsDeseados.Select(c => (c.Cron, c.UnidadTiempoAntelacion?.Id, c.CantAntelacion, c.EsVencimiento))];

			List<NormaSuscritaProcesoNotificacion> aEliminar = [];
			foreach (NormaSuscritaProcesoNotificacion existente in procesosNotificacion.Where(p => p.ProcesoAutomatico != null && p.ProcesoAutomatico.Cron != null)) {
				EntKairosParametrosProceso parametros = JsonSerializer.Deserialize(existente.ProcesoAutomatico!.Parametros, AppJsonSerializerContext.Default.EntKairosParametrosProceso)!;
				if (!deseados.Contains((existente.ProcesoAutomatico!.Cron!, parametros.IdTipoUnidadTiempoAntelacion, parametros.CantAntelacion, parametros.EsVencimiento ?? false))) {
					aEliminar.Add(existente);
				}
			}
			return aEliminar;
		}

		public List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> ExtraerCronsACrear(List<NormaSuscritaProcesoNotificacion> procesosNotificacion, List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> cronsDeseados) {
			HashSet<(string Cron, long? IdUnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> existentes = [.. procesosNotificacion
				.Where(p => p.ProcesoAutomatico != null && p.ProcesoAutomatico.Cron != null)
				.Select(p => {
					EntKairosParametrosProceso parametros = JsonSerializer.Deserialize(p.ProcesoAutomatico!.Parametros, AppJsonSerializerContext.Default.EntKairosParametrosProceso)!;
					return (p.ProcesoAutomatico!.Cron!, parametros.IdTipoUnidadTiempoAntelacion, parametros.CantAntelacion, parametros.EsVencimiento ?? false);
				})
			];

			List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> aCrear = [];
			foreach ((string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento) deseado in cronsDeseados) {
				if (!existentes.Contains((deseado.Cron, deseado.UnidadTiempoAntelacion?.Id, deseado.CantAntelacion, deseado.EsVencimiento))) {
					aCrear.Add(deseado);
				}
			}
			return aCrear;
		}

		public List<NormaSuscritaProcesoNotificacion> ExtraerFrecuenciasDiasAEliminar(List<NormaSuscritaProcesoNotificacion> procesosNotificacion, List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> frecuenciasDiasDeseadas) {
			HashSet<(int FrecuenciaDias, DateTime InicioEjecucionUtc, long? IdUnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> deseados = [.. frecuenciasDiasDeseadas.Select(c => (c.FrecuenciaDias, c.InicioEjecucionUtc, c.UnidadTiempoAntelacion?.Id, c.CantAntelacion, c.EsVencimiento))];

			List<NormaSuscritaProcesoNotificacion> aEliminar = [];
			foreach (NormaSuscritaProcesoNotificacion existente in procesosNotificacion.Where(p => p.ProcesoAutomatico != null && p.ProcesoAutomatico!.FrecuenciaDias != null && p.ProcesoAutomatico!.InicioEjecucionUtc != null)) {
				EntKairosParametrosProceso parametros = JsonSerializer.Deserialize(existente.ProcesoAutomatico!.Parametros, AppJsonSerializerContext.Default.EntKairosParametrosProceso)!;
				if (!deseados.Contains((existente.ProcesoAutomatico!.FrecuenciaDias!.Value, existente.ProcesoAutomatico!.InicioEjecucionUtc!.Value, parametros.IdTipoUnidadTiempoAntelacion, parametros.CantAntelacion, parametros.EsVencimiento ?? false))) {
					aEliminar.Add(existente);
				}
			}
			return aEliminar;
		}

		public List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> ExtraerFrecuenciasDiasACrear(List<NormaSuscritaProcesoNotificacion> procesosNotificacion, List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> frecuenciasDiasDeseadas) {
			HashSet<(int FrecuenciaDias, DateTime InicioEjecucionUtc, long? IdUnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> existentes = [.. procesosNotificacion
				.Where(p => p.ProcesoAutomatico != null && p.ProcesoAutomatico.FrecuenciaDias != null && p.ProcesoAutomatico.InicioEjecucionUtc != null)
				.Select(p => {
					EntKairosParametrosProceso parametros = JsonSerializer.Deserialize(p.ProcesoAutomatico!.Parametros, AppJsonSerializerContext.Default.EntKairosParametrosProceso)!;
					return (p.ProcesoAutomatico!.FrecuenciaDias!.Value, p.ProcesoAutomatico!.InicioEjecucionUtc!.Value, parametros.IdTipoUnidadTiempoAntelacion, parametros.CantAntelacion, parametros.EsVencimiento ?? false);
				})
			];

			List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> aCrear = [];
			foreach ((int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento) deseado in frecuenciasDiasDeseadas) {
				if (!existentes.Contains((deseado.FrecuenciaDias, deseado.InicioEjecucionUtc, deseado.UnidadTiempoAntelacion?.Id, deseado.CantAntelacion, deseado.EsVencimiento))) {
					aCrear.Add(deseado);
				}
			}
			return aCrear;
		}

		public async Task<List<NormaSuscritaProcesoNotificacion>> ObtenerPorNormaSuscrita(long idNormaSuscrita, bool filtrarVigente = false, NpgsqlTransaction? transaction = null) {
			List<NormaSuscritaProcesoNotificacion> items = await normaSuscritaProcesoNotificacionBcp.ObtenerPorNormaSuscrita(idNormaSuscrita, filtrarVigente: filtrarVigente, transaction: transaction);
			Dictionary<long, ProcesoAutomatico> procesos = (await procesoAutomaticoBcp.ObtenerVarios([.. items.Select(i => i.IdProcesoAutomatico)], filtrarVigente: filtrarVigente, transaction: transaction)).ToDictionary(p => p.Id, p => p);
			items.RemoveAll(item => {
				item.ProcesoAutomatico = procesos.GetValueOrDefault(item.IdProcesoAutomatico);
				return item.ProcesoAutomatico == null;
			});
			return items;
		}

		public async Task<NormaSuscritaProcesoNotificacion> RegistrarProcesoNotificacion(long idNormaSuscrita, string idProcesoKairos, string idCalendarizacionKairos, string nombre, string arnRol, string arnProceso, string parametros, string? cron, int? frecuenciaDias, DateTime? inicioEjecucionUtc, IDatabaseTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			IDatabaseConnection? connection = null;
			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexionWrapper();
					transaction = await connection.BeginTransactionAsync();
				}

				ProcesoAutomatico procesoAutomatico = await procesoAutomaticoBcp.Crear(
					ID_TIPO_PROCESO_NOTIFICACION,
					idProcesoKairos,
					idCalendarizacionKairos,
					nombre,
					arnRol,
					arnProceso,
					parametros,
					cron,
					frecuenciaDias,
					inicioEjecucionUtc,
					transaction!.NpgsqlTransaction()
				);

				NormaSuscritaProcesoNotificacion normaSuscritaProcesoNotificacion = await normaSuscritaProcesoNotificacionBcp.Crear(
					idNormaSuscrita,
					procesoAutomatico.Id,
					transaction!.NpgsqlTransaction()
				);
				normaSuscritaProcesoNotificacion.ProcesoAutomatico = procesoAutomatico;

				if (ownsTransaction) {
					await transaction!.CommitAsync();
				}

				return normaSuscritaProcesoNotificacion;
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

		public async Task EliminarProcesoNotificacion(NormaSuscritaProcesoNotificacion normaSuscritaProceso, IDatabaseTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			IDatabaseConnection? connection = null;
			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexionWrapper();
					transaction = await connection.BeginTransactionAsync();
				}

				if (normaSuscritaProceso.ProcesoAutomatico == null) throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "No se incluyó el proceso automático asociado al proceso de notificación", "El proceso de notificación es inválido.");

				await procesoAutomaticoBcp.Eliminar(normaSuscritaProceso.ProcesoAutomatico, transaction!.NpgsqlTransaction());
				await normaSuscritaProcesoNotificacionBcp.Eliminar(normaSuscritaProceso, transaction!.NpgsqlTransaction());

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

		public async Task ReversarProcesosProgramadosDesprogramados(List<SalKairosIngresarProceso> procesosProgramados, List<NormaSuscritaProcesoNotificacion> procesosDesprogramados) {
			await kairosHelper.IngresarVariosProcesos([.. procesosDesprogramados.Select(p => {
				ProcesoAutomatico pa = p.ProcesoAutomatico ?? throw new InvalidOperationException("El proceso automático no puede ser nulo");
				return new EntKairosIngresarProceso() {
					Nombre = pa.Nombre,
					Cron = pa.Cron,
					FrecuenciaDias = pa.FrecuenciaDias,
					InicioEjecucionUtc = pa.InicioEjecucionUtc,
					ArnRol = pa.ArnRol,
					ArnProceso = pa.ArnProceso,
					Parametros = pa.Parametros
				};
			})]);
			await kairosHelper.EliminarVariosProcesos([.. procesosProgramados.Select(p => p.IdProceso)]);
		}

		public async Task<(List<SalKairosIngresarProceso> procesosCronProgramados, List<NormaSuscritaProcesoNotificacion> procesosCronDesprogramados)> ActualizarProcesosNotificacionesCron(NormaSuscrita normaSuscrita, List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> cronsDeseados, IDatabaseTransaction? transaction = null) {
			List<SalKairosIngresarProceso> procesosProgramados = [];
			List<NormaSuscritaProcesoNotificacion> procesosDesprogramados = [];

			bool ownsTransaction = transaction == null;
			IDatabaseConnection? connection = null;
			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexionWrapper();
					transaction = await connection.BeginTransactionAsync();
				}

				if (normaSuscrita.NormaSuscritaProcesosNotificaciones == null) throw new InvalidOperationException("La norma suscrita debe incluir sus procesos de notificaciones actuales.");

				List<NormaSuscritaProcesoNotificacion> cronsAEliminar = ExtraerCronsAEliminar(normaSuscrita.NormaSuscritaProcesosNotificaciones, cronsDeseados);
				List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> cronsACrear = ExtraerCronsACrear(normaSuscrita.NormaSuscritaProcesosNotificaciones, cronsDeseados);

				await kairosHelper.EliminarVariosProcesos([.. cronsAEliminar.Select(c => c.ProcesoAutomatico!.IdProcesoKairos)]);
				procesosDesprogramados.AddRange(cronsAEliminar);
				foreach (NormaSuscritaProcesoNotificacion eliminar in cronsAEliminar) {
					await EliminarProcesoNotificacion(eliminar, transaction);
					normaSuscrita.NormaSuscritaProcesosNotificaciones.RemoveAll(p => p.Id == eliminar.Id);
				}

				List<SalKairosIngresarProceso> retornos = await kairosHelper.IngresarVariosProcesos([.. cronsACrear.Select(crear => new EntKairosIngresarProceso {
					Nombre = $"{APP_NAME} - NormaSuscrita {normaSuscrita.Id} - Cron {crear.Cron}",
					Cron = crear.Cron,
					Parametros = JsonSerializer.Serialize(new EntKairosParametrosProceso() {
						IdNormaSuscrita = normaSuscrita.Id,
						Cron = crear.Cron,
						IdTipoUnidadTiempoAntelacion = crear.UnidadTiempoAntelacion?.Id,
						CantAntelacion = crear.CantAntelacion,
						EsVencimiento = crear.EsVencimiento,
						ProgramarSiguienteEjecucion = crear.EsVencimiento
					}, AppJsonSerializerContext.Default.EntKairosParametrosProceso),
					ArnProceso = NOTIFICACIONES_LAMBDA_ARN,
					ArnRol = NOTIFICACIONES_EJECUCION_ROLE_ARN
				})]);
				procesosProgramados.AddRange(retornos);
				foreach (SalKairosIngresarProceso retorno in retornos) {
					normaSuscrita.NormaSuscritaProcesosNotificaciones.Add(
						await RegistrarProcesoNotificacion(
							normaSuscrita.Id,
							retorno.IdProceso,
							retorno.IdCalendarizacion,
							retorno.Nombre,
							retorno.ArnRol,
							retorno.ArnProceso,
							retorno.Parametros,
							retorno.Cron,
							retorno.FrecuenciaDias,
							retorno.InicioEjecucionUtc,
							transaction
						)
					);
				}
				
				if (ownsTransaction) {
					await transaction!.CommitAsync();
				}

				return (procesosProgramados, procesosDesprogramados);
			} catch {
				await ReversarProcesosProgramadosDesprogramados(procesosProgramados, procesosDesprogramados);
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

		public async Task<(List<SalKairosIngresarProceso> frecuenciasDiasProgramados, List<NormaSuscritaProcesoNotificacion> frecuenciasDiasDesprogramadas)> ActualizarProcesosNotificacionesFrecuenciaDias(NormaSuscrita normaSuscrita, List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> frecuenciasDiasDeseadas, IDatabaseTransaction? transaction = null) {
			List<SalKairosIngresarProceso> procesosProgramados = [];
			List<NormaSuscritaProcesoNotificacion> procesosDesprogramados = [];

			bool ownsTransaction = transaction == null;
			IDatabaseConnection? connection = null;
			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexionWrapper();
					transaction = await connection.BeginTransactionAsync();
				}

				if (normaSuscrita.NormaSuscritaProcesosNotificaciones == null) throw new InvalidOperationException("La norma suscrita debe incluir sus procesos de notificaciones actuales.");

				List<NormaSuscritaProcesoNotificacion> frecuenciasDiasAEliminar = ExtraerFrecuenciasDiasAEliminar(normaSuscrita.NormaSuscritaProcesosNotificaciones, frecuenciasDiasDeseadas);
				List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> frecuenciasDiasACrear = ExtraerFrecuenciasDiasACrear(normaSuscrita.NormaSuscritaProcesosNotificaciones, frecuenciasDiasDeseadas);

				await kairosHelper.EliminarVariosProcesos([.. frecuenciasDiasAEliminar.Select(c => c.ProcesoAutomatico!.IdProcesoKairos)]);
				procesosDesprogramados.AddRange(frecuenciasDiasAEliminar);
				foreach (NormaSuscritaProcesoNotificacion eliminar in frecuenciasDiasAEliminar) {
					await EliminarProcesoNotificacion(eliminar, transaction);
					normaSuscrita.NormaSuscritaProcesosNotificaciones.RemoveAll(p => p.Id == eliminar.Id);
				}

				List<SalKairosIngresarProceso> retornos = await kairosHelper.IngresarVariosProcesos([.. frecuenciasDiasACrear.Select(crear => {
					DateTime inicioEjecucionChile = DateTimeHelper.TransformarFechaUTCATimezone(crear.InicioEjecucionUtc);
					return new EntKairosIngresarProceso {
						Nombre = $"{APP_NAME} - NormaSuscrita {normaSuscrita.Id} - Inicio {inicioEjecucionChile:dd-MM-yyyy HH:mm} - Frecuencia {crear.FrecuenciaDias} Días",
						FrecuenciaDias = crear.FrecuenciaDias,
						InicioEjecucionUtc = crear.InicioEjecucionUtc,
						Parametros = JsonSerializer.Serialize(new EntKairosParametrosProceso() {
							IdNormaSuscrita = normaSuscrita.Id,
							FrecuenciaDias = crear.FrecuenciaDias,
							InicioEjecucionUtc = crear.InicioEjecucionUtc,
							IdTipoUnidadTiempoAntelacion = crear.UnidadTiempoAntelacion?.Id,
							CantAntelacion = crear.CantAntelacion,
							EsVencimiento = crear.EsVencimiento,
							ProgramarSiguienteEjecucion = crear.EsVencimiento
						}, AppJsonSerializerContext.Default.EntKairosParametrosProceso),
						ArnProceso = NOTIFICACIONES_LAMBDA_ARN,
						ArnRol = NOTIFICACIONES_EJECUCION_ROLE_ARN
					};
				})]);
				procesosProgramados.AddRange(retornos);
				foreach (SalKairosIngresarProceso retorno in retornos) {
					normaSuscrita.NormaSuscritaProcesosNotificaciones.Add(
						await RegistrarProcesoNotificacion(
							normaSuscrita.Id,
							retorno.IdProceso,
							retorno.IdCalendarizacion,
							retorno.Nombre,
							retorno.ArnRol,
							retorno.ArnProceso,
							retorno.Parametros,
							retorno.Cron,
							retorno.FrecuenciaDias,
							retorno.InicioEjecucionUtc,
							transaction
						)
					);
				}

				if (ownsTransaction) {
					await transaction!.CommitAsync();
				}

				return (procesosProgramados, procesosDesprogramados);
			} catch {
				await ReversarProcesosProgramadosDesprogramados(procesosProgramados, procesosDesprogramados);
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

		public async Task<(List<SalKairosIngresarProceso> procesosProgramados, List<NormaSuscritaProcesoNotificacion> procesosDesprogramados)> ActualizarProcesosNotificacionesNormaSuscrita(NormaSuscrita normaSuscrita, List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> cronsDeseados, List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> frecuenciasDiasDeseadas, IDatabaseTransaction? transaction = null) {
			List<SalKairosIngresarProceso> procesosProgramados = [];
			List<NormaSuscritaProcesoNotificacion> procesosDesprogramados = [];

			bool ownsTransaction = transaction == null;
			IDatabaseConnection? connection = null;
			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexionWrapper();
					transaction = await connection.BeginTransactionAsync();
				}

				if (normaSuscrita.NormaSuscritaProcesosNotificaciones == null) throw new InvalidOperationException("La norma suscrita debe incluir sus procesos de notificaciones actuales.");

				(List<SalKairosIngresarProceso> cronsProgramados, List<NormaSuscritaProcesoNotificacion> cronsDesprogramados) = await ActualizarProcesosNotificacionesCron(normaSuscrita, cronsDeseados, transaction);
				procesosProgramados.AddRange(cronsProgramados);
				procesosDesprogramados.AddRange(cronsDesprogramados);

				(List<SalKairosIngresarProceso> frecuenciasDiasProgramados, List<NormaSuscritaProcesoNotificacion> frecuenciasDiasDesprogramadas) = await ActualizarProcesosNotificacionesFrecuenciaDias(normaSuscrita, frecuenciasDiasDeseadas, transaction);
				procesosProgramados.AddRange(frecuenciasDiasProgramados);
				procesosDesprogramados.AddRange(frecuenciasDiasDesprogramadas);

				if (ownsTransaction) {
					await transaction!.CommitAsync();
				}

				return (procesosProgramados, procesosDesprogramados);
			} catch {
				await ReversarProcesosProgramadosDesprogramados(procesosProgramados, procesosDesprogramados);
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
