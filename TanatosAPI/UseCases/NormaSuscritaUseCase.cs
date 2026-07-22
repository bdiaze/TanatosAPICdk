using Cronos;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using Scriban.Runtime;
using System.Net;
using System.Text.Json;
using TanatosAPI.Business;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Hermes;
using TanatosAPI.Entities.Others.Kairos;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.UseCases {
	public class NormaSuscritaUseCase(IDatabaseConnectionHelper connectionHelper, IDateTimeProvider dateTimeProvider, HistorialNormaSuscritaUseCase historialNormaSuscritaUseCase, NotificacionNormaSuscritaUseCase notificacionNormaSuscritaUseCase, INormaSuscritaBcp normaSuscritaBcp, ITemplateNormaBcp templateNormaBcp, IHistorialNormaSuscritaBcp historialNormaSuscritaBcp, IHistorialNotificacionBcp historialNotificacionBcp, IFiscalizadorNormaSuscritaBcp fiscalizadorNormaSuscritaBcp, INotificacionNormaSuscritaBcp notificacionNormaSuscritaBcp, ITipoPeriodicidadBcp tipoPeriodicidadBcp, ICategoriaNormaBcp categoriaNormaBcp, ITipoFiscalizadorBcp tipoFiscalizadorBcp, ITipoUnidadTiempoBcp tipoUnidadTiempoBcp, ICargoBcp cargoBcp, INegocioBcp negocioBcp, ISuscripcionBcp suscripcionBcp) {
		public async Task<(NormaSuscrita?, TemplateNorma?)> ObtenerPorIdConTemplate(long idNormaSuscrita, NpgsqlTransaction? transaction = null) {
			NormaSuscrita? normaSuscrita = await normaSuscritaBcp.ObtenerPorId(idNormaSuscrita, transaction);
			TemplateNorma? templateNorma = null;
			if (normaSuscrita?.IdTemplate != null && normaSuscrita?.IdNorma != null) {
				templateNorma = await templateNormaBcp.ObtenerPorTemplateNorma(normaSuscrita.IdTemplate.Value, normaSuscrita.IdNorma.Value, transaction);
			}
			return (normaSuscrita, templateNorma);
		}
				
		public async Task ActualizarProgramacionProcesosNormaSuscrita(long idNormaSuscrita, NpgsqlTransaction? transaction = null) {
			List<ProcesoNotificacion> procesosProgramados = [];
			List<ProcesoNotificacion> procesosDesprogramados = [];
			try {
				(NormaSuscrita? normaSuscrita, TemplateNorma? templateNorma) = await ObtenerPorIdConTemplate(idNormaSuscrita, transaction);
				if (normaSuscrita == null) throw new InvalidOperationException("Norma suscrita inválida");

				List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> cronsDeseados = [];
				List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> frecuenciasDiasDeseadas = [];

                // Si la norma suscrita está activada, se obtienen los crons y frecuencias deseados...
                if (normaSuscrita.Activado) {
					long idTipoPeriodicidad = (normaSuscrita.IdTipoPeriodicidad ?? templateNorma?.IdTipoPeriodicidad) ?? throw new InvalidOperationException("Obligación sin tipo de periodicidad asociada");
					TipoPeriodicidad? tipoPeriodicidad = await tipoPeriodicidadBcp.ObtenerPorId(idTipoPeriodicidad, transaction);

					// Solo se obtienen los crons y frecuencias deseados si la periodicidad está vigente...
					if (tipoPeriodicidadBcp.EstaVigente(tipoPeriodicidad)) {
						// Se obtienen las notificaciones previas configuradas para la norma suscrita...
						List<(TipoUnidadTiempo UnidadTiempo, int CantAntelacion)> antelaciones = await notificacionNormaSuscritaUseCase.ObtenerAntelacionesConsiderandoTemplate(normaSuscrita.Id, normaSuscrita.IdTemplate, normaSuscrita.IdNorma, transaction);
							
						// Se calculan los crons y frecuencias según el próximo vencimiento...
						DateTime proximoVencimiento = await historialNormaSuscritaBcp.ObtenerProximoVencimiento(normaSuscrita.Id, transaction);
						if (!string.IsNullOrWhiteSpace(tipoPeriodicidad!.Cron)) {
							cronsDeseados = await notificacionNormaSuscritaUseCase.GenerarCrons(
								proximoVencimiento,
								tipoPeriodicidad!.Cron,
								antelaciones
							);
						} else if (tipoPeriodicidad!.FrecuenciaDias != null) {
                            frecuenciasDiasDeseadas = await notificacionNormaSuscritaUseCase.GenerarFrecuenciasDias(
                                proximoVencimiento,
                                tipoPeriodicidad!.FrecuenciaDias.Value,
                                antelaciones
                            );
                        }
					}
				}

                (List<ProcesoNotificacion> cronsProgramados, List<ProcesoNotificacion> cronsDesprogramados) = await normaSuscritaBcp.ActualizarProcesosCronProgramados(normaSuscrita, cronsDeseados);
                procesosProgramados.AddRange(cronsProgramados);
                procesosDesprogramados.AddRange(cronsDesprogramados);

				(List<ProcesoNotificacion> frecuenciasDiasProgramados, List<ProcesoNotificacion> frecuenciasDiasDesprogramadas) = await normaSuscritaBcp.ActualizarProcesosFrecuenciaDiasProgramados(normaSuscrita, frecuenciasDiasDeseadas);
                procesosProgramados.AddRange(frecuenciasDiasProgramados);
                procesosDesprogramados.AddRange(frecuenciasDiasDesprogramadas);
			} catch {
				await normaSuscritaBcp.ReversarProcesos(procesosProgramados, procesosDesprogramados);
				throw;
			}
		}

		public async Task EliminarNormaSuscrita(NormaSuscrita normaSuscrita, NpgsqlTransaction transaction) {
			if (normaSuscrita.Vigencia) {
				await normaSuscritaBcp.Eliminar(normaSuscrita);

				await ActualizarProgramacionProcesosNormaSuscrita(normaSuscrita.Id, transaction);

				await fiscalizadorNormaSuscritaBcp.EliminarPorNormaSuscrita(normaSuscrita.Id, transaction);
				await notificacionNormaSuscritaBcp.EliminarPorNormaSuscrita(normaSuscrita.Id, transaction);
				await historialNormaSuscritaUseCase.EliminarPorNormaSuscrita(normaSuscrita.Id, false, transaction);
			}
		}

        public async Task EliminarNormaValidandoPertenencia(string sub, long idNormaSuscrita, IDatabaseTransaction? transaction = null) {
            bool ownsTransaction = transaction == null;
            IDatabaseConnection? connection = null;
            try {
                if (ownsTransaction) {
                    connection = await connectionHelper.ObtenerConexionWrapper();
                    transaction = await connection.BeginTransactionAsync();
                }

                NormaSuscrita? obligacion = await normaSuscritaBcp.ObtenerSiVigenteValidandoPertenenciaYEditable(idNormaSuscrita, sub, transaction!.NpgsqlTransaction());
                if (obligacion != null) {
                    await EliminarNormaSuscrita(obligacion, transaction!.NpgsqlTransaction());
                }

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

        public async Task<(NormaSuscrita obligacion, TipoPeriodicidad? periodicidad, CategoriaNorma? categoriaNorma, Cargo? cargo, List<FiscalizadorNormaSuscrita> fiscalizadores, List<NotificacionNormaSuscrita> antelaciones, HistorialNormaSuscrita? proximoVencimiento)> CrearNormaSuscrita(string sub, long idNegocio, string nombre, string? descripcion, string? multa, long? idTipoPeriodicidad, long? idCategoriaNorma, long? idCargo, bool activado, DateTime? proximoVencimiento, HashSet<long> idFiscalizadores, HashSet<(long IdTipoUnidadTiempo, int CantAntelacion)> antelaciones, IDatabaseTransaction? transaction = null) {
            bool ownsTransaction = transaction == null;
            IDatabaseConnection? connection = null;
            try {
                if (ownsTransaction) {
                    connection = await connectionHelper.ObtenerConexionWrapper();
                    transaction = await connection.BeginTransactionAsync();
                }

                bool tienePlanEmpresa = await suscripcionBcp.ConsultaTienePlanEmpresa(sub, transaction!.NpgsqlTransaction());
                if (!tienePlanEmpresa && idCargo != null) throw new ErrorValidacion(TipoErrorValidacion.RestringidoPorPlan, "Tu plan no permite asignar un cargo responsable a la obligación.");

                Dictionary<long, TipoFiscalizador> tiposFiscalizadores = (await tipoFiscalizadorBcp.ValidarTodosVigentes(idFiscalizadores)).ToDictionary(f => f.Id, f => f);
                Dictionary<long, TipoUnidadTiempo> tiposUnidadesTiempo = (await tipoUnidadTiempoBcp.ValidarTodosVigentes([.. antelaciones.Select(a => a.IdTipoUnidadTiempo)])).ToDictionary(u => u.Id, u => u);
                if (antelaciones.Any(a => a.CantAntelacion <= 0)) throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "Una notificación con cantidad antelación inválido.");
                if (activado) {
                    if (proximoVencimiento == null) throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "Debe incluir la fecha de próximo vencimiento.");
                    if (proximoVencimiento!.Value <= dateTimeProvider.UtcNow) throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "El próximo vencimiento debe ser una fecha futura.");
                }

                Negocio _ = await negocioBcp.ObtenerValidandoVigenciaYPertenencia(idNegocio, sub, transaction!.NpgsqlTransaction());

                TipoPeriodicidad? periodicidad = null;
                if (idTipoPeriodicidad != null) periodicidad = await tipoPeriodicidadBcp.ObtenerValidandoVigencia(idTipoPeriodicidad, transaction!.NpgsqlTransaction());

                CategoriaNorma? categoriaNorma = null;
                if (idCategoriaNorma != null) categoriaNorma = await categoriaNormaBcp.ObtenerValidandoVigencia(idCategoriaNorma, transaction!.NpgsqlTransaction());

                Cargo? cargo = null;
                if (idCargo != null) cargo = await cargoBcp.ObtenerValidandoVigenciaPertenenciaNegocio(idCargo.Value, idNegocio, sub, transaction!.NpgsqlTransaction());

                NormaSuscrita obligacion = await normaSuscritaBcp.CrearObligacionUsuario(sub, idNegocio, nombre, descripcion, multa, idTipoPeriodicidad, idCategoriaNorma, idCargo, activado, transaction!.NpgsqlTransaction());
                List<FiscalizadorNormaSuscrita> fiscalizadoresNormaSuscrita = await fiscalizadorNormaSuscritaBcp.ActualizarPorNormaSuscrita(obligacion.Id, idFiscalizadores, transaction!.NpgsqlTransaction());
                fiscalizadoresNormaSuscrita = [.. fiscalizadoresNormaSuscrita.Select(f => {
                    f.TipoFiscalizador = tiposFiscalizadores[f.IdTipoFiscalizador];
                    return f;
                })];
                
                List<NotificacionNormaSuscrita> notificacionesNormaSuscrita = await notificacionNormaSuscritaBcp.ActualizarPorNormaSuscrita(obligacion.Id, antelaciones, transaction!.NpgsqlTransaction());
                notificacionesNormaSuscrita = [.. notificacionesNormaSuscrita.Select(f => {
                    f.TipoUnidadTiempo = tiposUnidadesTiempo[f.IdTipoUnidadTiempoAntelacion];
                    return f;
                })];

                HistorialNormaSuscrita? vencimiento = null;
                if (activado && proximoVencimiento != null) vencimiento = await historialNormaSuscritaBcp.Crear(obligacion.Id, proximoVencimiento.Value, transaction!.NpgsqlTransaction());
                await ActualizarProgramacionProcesosNormaSuscrita(obligacion.Id, transaction!.NpgsqlTransaction());

                if (ownsTransaction) {
                    await transaction!.CommitAsync();
                }

                return (obligacion, periodicidad, categoriaNorma, cargo, fiscalizadoresNormaSuscrita, notificacionesNormaSuscrita, vencimiento);
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

		public async Task<(long idObligacion, long idVencimiento, DateTime fechaCompletitud)> CompletarNormaValidandoPertenencia(string sub, long idNormaSuscrita, long idHistorialNormaSuscrita, IDatabaseTransaction? transaction = null) {
            bool ownsTransaction = transaction == null;
            IDatabaseConnection? connection = null;
            try {
                if (ownsTransaction) {
                    connection = await connectionHelper.ObtenerConexionWrapper();
                    transaction = await connection.BeginTransactionAsync();
                }

                NormaSuscrita obligacion = await normaSuscritaBcp.ObtenerValidandoVigenciaYPertenencia(idNormaSuscrita, sub, transaction!.NpgsqlTransaction());
                HistorialNormaSuscrita vencimiento = await historialNormaSuscritaBcp.ObtenerValidandoVigenciaYPertenencia(idHistorialNormaSuscrita, idNormaSuscrita, transaction!.NpgsqlTransaction());
                
                DateTime fechaCompletitud = await historialNormaSuscritaUseCase.CompletarHistorialNormaSuscrita(vencimiento, transaction!.NpgsqlTransaction());

                if (ownsTransaction) {
                    await transaction!.CommitAsync();
                }

                return (obligacion.Id, vencimiento.Id, fechaCompletitud);
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

		public async Task<(long idObligacion, long idVencimiento, DateTime fechaCompletitud)> CompletarNormaPorCodigoAcceso(string codigoAcceso, IDatabaseTransaction? transaction = null) {
            bool ownsTransaction = transaction == null;
            IDatabaseConnection? connection = null;
            try {
                if (ownsTransaction) {
                    connection = await connectionHelper.ObtenerConexionWrapper();
                    transaction = await connection.BeginTransactionAsync();
                }

                HistorialNotificacion historialNotificacion = await historialNotificacionBcp.ObtenerPorCodigoAccesoValidandoVigencia(codigoAcceso, transaction!.NpgsqlTransaction());
				HistorialNormaSuscrita vencimiento = await historialNormaSuscritaBcp.ObtenerValidandoVigencia(historialNotificacion.IdHistorialNormaSuscrita, transaction!.NpgsqlTransaction());
				NormaSuscrita obligacion = await normaSuscritaBcp.ObtenerValidandoVigencia(vencimiento.IdNormaSuscrita, transaction!.NpgsqlTransaction());

				DateTime fechaCompletitud = await historialNormaSuscritaUseCase.CompletarHistorialNormaSuscrita(vencimiento, transaction!.NpgsqlTransaction());

                if (ownsTransaction) {
                    await transaction!.CommitAsync();
                }

				return (obligacion.Id, vencimiento.Id, fechaCompletitud);
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
