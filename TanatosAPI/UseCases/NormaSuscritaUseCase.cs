using Actions_Compile;
using Amazon.S3.Model;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using Org.BouncyCastle.Crypto.Digests;
using System.Data.Common;
using System.Linq.Expressions;
using System.Text.Json;
using System.Transactions;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Kairos;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.UseCases;
using TanatosAPI.Repositories;

namespace TanatosAPI.UseCases {
	public class NormaSuscritaUseCase(IDatabaseConnectionHelper connectionHelper, IDateTimeProvider dateTimeProvider, IHistorialNormaSuscritaUseCase historialNormaSuscritaUseCase, INotificacionNormaSuscritaUseCase notificacionNormaSuscritaUseCase, INormaSuscritaProcesoNotificacionUseCase normaSuscritaProcesoNotificacionUseCase, INormaSuscritaBcp normaSuscritaBcp, IHistorialNormaSuscritaBcp historialNormaSuscritaBcp, IHistorialNotificacionBcp historialNotificacionBcp, IFiscalizadorNormaSuscritaBcp fiscalizadorNormaSuscritaBcp, INotificacionNormaSuscritaBcp notificacionNormaSuscritaBcp, ITemplateBcp templateBcp, ITemplateNormaBcp templateNormaBcp, ITemplateNormaNotificacionBcp templateNormaNotificacionBcp, ITemplateNormaFiscalizadorBcp templateNormaFiscalizadorBcp, ITipoPeriodicidadBcp tipoPeriodicidadBcp, ICategoriaNormaBcp categoriaNormaBcp, ITipoFiscalizadorBcp tipoFiscalizadorBcp, ITipoUnidadTiempoBcp tipoUnidadTiempoBcp, ICargoBcp cargoBcp, INegocioBcp negocioBcp, ISuscripcionBcp suscripcionBcp, IDocumentoAdjuntoBcp documentoAdjuntoBcp) {
		public async Task IncluirTemplate(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null) {
			await IncluirTemplate([normaSuscrita], transaction);
        }

		public async Task IncluirTemplate(List<NormaSuscrita> normasSuscritas, NpgsqlTransaction? transaction = null) {
			if (normasSuscritas.Any(o => o.IdTemplate != null)) {
				List<Template> templates = await templateBcp.ObtenerVariosSoloVigentes([.. normasSuscritas.Where(o => o.IdTemplate != null).Select(o => o.IdTemplate!.Value)], transaction);

				if (templates.Count > 0) {
					Dictionary<(long idTemplate, long idNorma), TemplateNorma> templatesNormas = [];
					foreach (Template template in templates) {
						foreach (TemplateNorma templateNorma in await templateNormaBcp.ObtenerPorTemplate(template.Id, transaction)) {
							templateNorma.Template = template;
							templatesNormas[(templateNorma.IdTemplate, templateNorma.IdNorma)] = templateNorma;
						}
					}

					foreach (NormaSuscrita normaSuscrita in normasSuscritas) {
						normaSuscrita.TemplateNorma = normaSuscrita.IdTemplate != null && normaSuscrita.IdNorma != null && templatesNormas.TryGetValue((normaSuscrita.IdTemplate.Value, normaSuscrita.IdNorma.Value), out TemplateNorma? tn) ? tn : null;
						normaSuscrita.IdTemplate = normaSuscrita.TemplateNorma?.IdTemplate;
						normaSuscrita.IdNorma = normaSuscrita.TemplateNorma?.IdNorma;
					}
				}
			}
		}

        public async Task IncluirTipoPeriodicidad(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null) {
			await IncluirTipoPeriodicidad([normaSuscrita], transaction);
		}

		public async Task IncluirTipoPeriodicidad(List<NormaSuscrita> normasSuscritas, NpgsqlTransaction? transaction = null) {
			if (normasSuscritas.Any(o => o.IdTipoPeriodicidad != null || o.TemplateNorma?.IdTipoPeriodicidad != null)) {
				Dictionary<long, TipoPeriodicidad> periodicidades = (await tipoPeriodicidadBcp.ObtenerVigentes(transaction)).ToDictionary(p => p.Id, p => p);

				foreach (NormaSuscrita normaSuscrita in normasSuscritas) {
					normaSuscrita.TipoPeriodicidad = normaSuscrita.IdTipoPeriodicidad != null && periodicidades.TryGetValue(normaSuscrita.IdTipoPeriodicidad.Value, out TipoPeriodicidad? tp) ? tp : null;
					normaSuscrita.IdTipoPeriodicidad = normaSuscrita.TipoPeriodicidad?.Id;

					if (normaSuscrita.TemplateNorma != null) {
						normaSuscrita.TemplateNorma.TipoPeriodicidad = normaSuscrita.TemplateNorma.IdTipoPeriodicidad != null && periodicidades.TryGetValue(normaSuscrita.TemplateNorma.IdTipoPeriodicidad.Value, out TipoPeriodicidad? tpt) ? tpt : null;
						normaSuscrita.TemplateNorma.IdTipoPeriodicidad = normaSuscrita.TemplateNorma.TipoPeriodicidad?.Id;
					}
				}
			}
		}

		public async Task IncluirCategoriaNorma(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null) {
			await IncluirCategoriaNorma([normaSuscrita], transaction);
		}

		public async Task IncluirCategoriaNorma(List<NormaSuscrita> normasSuscritas, NpgsqlTransaction? transaction = null) {
			if (normasSuscritas.Any(o => o.IdCategoriaNorma != null || o.TemplateNorma?.IdCategoriaNorma != null)) {
				Dictionary<long, CategoriaNorma> categorias = (await categoriaNormaBcp.ObtenerVigentes(transaction)).ToDictionary(p => p.Id, p => p);

				foreach (NormaSuscrita normaSuscrita in normasSuscritas) {
					normaSuscrita.CategoriaNorma = normaSuscrita.IdCategoriaNorma != null && categorias.TryGetValue(normaSuscrita.IdCategoriaNorma.Value, out CategoriaNorma? cn) ? cn : null;
					normaSuscrita.IdCategoriaNorma = normaSuscrita.CategoriaNorma?.Id;

					if (normaSuscrita.TemplateNorma != null) {
						normaSuscrita.TemplateNorma.CategoriaNorma = categorias.TryGetValue(normaSuscrita.TemplateNorma.IdCategoriaNorma, out CategoriaNorma? cnt) ? cnt : null;
						normaSuscrita.TemplateNorma.IdCategoriaNorma = normaSuscrita.TemplateNorma.CategoriaNorma?.Id ?? normaSuscrita.TemplateNorma.IdCategoriaNorma;
					}
				}
			}
		}

		public async Task IncluirCargo(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null) {
			await IncluirCargo([normaSuscrita], transaction);
		}

		public async Task IncluirCargo(List<NormaSuscrita> normasSuscritas, NpgsqlTransaction? transaction = null) {
			if (normasSuscritas.Any(o => o.IdCargo != null)) {
				Dictionary<(string sub, long idNegocio), Dictionary<long, Cargo>> cargos = [];
				foreach ((string sub, long idNegocio) in normasSuscritas.Select(ns => (ns.Sub, ns.IdNegocio)).ToHashSet()) {
					cargos[(sub, idNegocio)] = (await cargoBcp.ObtenerPorSubYNegocio(sub, idNegocio, filtrarVigente: true, transaction: transaction)).ToDictionary(p => p.Id, p => p);
				}

				foreach (NormaSuscrita normaSuscrita in normasSuscritas) {
					normaSuscrita.Cargo = normaSuscrita.IdCargo != null && cargos[(normaSuscrita.Sub, normaSuscrita.IdNegocio)].TryGetValue(normaSuscrita.IdCargo.Value, out Cargo? c) ? c : null;
					normaSuscrita.IdCargo = normaSuscrita.Cargo?.Id;
				}
			}
		}

		public async Task IncluirFiscalizadores(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null) {
			normaSuscrita.FiscalizadoresNormaSuscrita = await fiscalizadorNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(normaSuscrita.Id, transaction);
			if (normaSuscrita.TemplateNorma != null) {
				normaSuscrita.TemplateNorma.TemplateNormaFiscalizadores = await templateNormaFiscalizadorBcp.ObtenerPorTemplateNorma(normaSuscrita.TemplateNorma.IdTemplate, normaSuscrita.TemplateNorma.IdNorma, transaction);
			}

			if (normaSuscrita.FiscalizadoresNormaSuscrita.Count > 0 || normaSuscrita.TemplateNorma?.TemplateNormaFiscalizadores?.Count > 0) {
				Dictionary<long, TipoFiscalizador> fiscalizadores = (await tipoFiscalizadorBcp.ObtenerVigentes(transaction)).ToDictionary(p => p.Id, p => p);

				foreach (FiscalizadorNormaSuscrita f in normaSuscrita.FiscalizadoresNormaSuscrita.ToList()) {
					f.TipoFiscalizador = fiscalizadores.TryGetValue(f.IdTipoFiscalizador, out TipoFiscalizador? tf) ? tf : null;
					if (f.TipoFiscalizador == null) normaSuscrita.FiscalizadoresNormaSuscrita.Remove(f);
				}

				foreach (TemplateNormaFiscalizador f in normaSuscrita.TemplateNorma?.TemplateNormaFiscalizadores?.ToList() ?? []) {
					f.TipoFiscalizador = fiscalizadores.TryGetValue(f.IdTipoFiscalizador, out TipoFiscalizador? tf) ? tf : null;
					if (f.TipoFiscalizador == null) normaSuscrita.TemplateNorma!.TemplateNormaFiscalizadores!.Remove(f);
				}
			}
		}

		public async Task IncluirNotificaciones(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null) {
			normaSuscrita.NotificacionesNormaSuscrita = await notificacionNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(normaSuscrita.Id, transaction);
			if (normaSuscrita.TemplateNorma != null) {
				normaSuscrita.TemplateNorma.TemplateNormaNotificaciones = await templateNormaNotificacionBcp.ObtenerPorTemplateNorma(normaSuscrita.TemplateNorma.IdTemplate, normaSuscrita.TemplateNorma.IdNorma, transaction);
			}

			if (normaSuscrita.NotificacionesNormaSuscrita.Count > 0 || normaSuscrita.TemplateNorma?.TemplateNormaNotificaciones?.Count > 0) {
				Dictionary<long, TipoUnidadTiempo> unidades = (await tipoUnidadTiempoBcp.ObtenerVigentes(transaction)).ToDictionary(p => p.Id, p => p);

				foreach (NotificacionNormaSuscrita f in normaSuscrita.NotificacionesNormaSuscrita.ToList()) {
					f.TipoUnidadTiempo = unidades.TryGetValue(f.IdTipoUnidadTiempoAntelacion, out TipoUnidadTiempo? ut) ? ut : null;
					if (f.TipoUnidadTiempo == null) normaSuscrita.NotificacionesNormaSuscrita.Remove(f);
				}

				foreach (TemplateNormaNotificacion f in normaSuscrita.TemplateNorma?.TemplateNormaNotificaciones?.ToList() ?? []) {
					f.TipoUnidadTiempoAntelacion = unidades.TryGetValue(f.IdTipoUnidadTiempoAntelacion, out TipoUnidadTiempo? ut) ? ut : null;
					if (f.TipoUnidadTiempoAntelacion == null) normaSuscrita.TemplateNorma!.TemplateNormaNotificaciones!.Remove(f);
				}
			}
		}

		public async Task IncluirHistorialVencimientos(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null) {
			await IncluirHistorialVencimientos([normaSuscrita], transaction);
		}

		public async Task IncluirHistorialVencimientos(List<NormaSuscrita> normasSuscritas, NpgsqlTransaction? transaction = null) {
			await Task.WhenAll(
				normasSuscritas.Select(async o => {
					if (normaSuscritaBcp.EstaActiva(o)) {
						o.HistorialesNormaSuscrita = await historialNormaSuscritaBcp.ObtenerPorNormaSuscrita(o.Id, filtrarVigente: true, transaction: transaction);
					} else {
						o.HistorialesNormaSuscrita = await historialNormaSuscritaBcp.ObtenerPorNormaSuscrita(o.Id, filtrarVigente: true, filtrarCompletadas: true, transaction: transaction);
					}
				})
			);
		}

		public async Task IncluirProcesosNotificaciones(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null) {
			normaSuscrita.NormaSuscritaProcesosNotificaciones = await normaSuscritaProcesoNotificacionUseCase.ObtenerPorNormaSuscrita(normaSuscrita.Id, filtrarVigente: true, transaction: transaction);
		}

		public async Task<NormaSuscrita?> Obtener(long idNormaSuscrita, bool validarVigencia = false, string? validarSub = null, long? validarIdNegocio = null, bool incluirTemplate = false, bool incluirPeriodicidad = false, bool incluirCategoria = false, bool incluirCargo = false, bool incluirFiscalizadores = false, bool incluirNotificaciones = false, bool incluirHistorialVencimientos = false, bool incluirProcesosNotificaciones = false, NpgsqlTransaction? transaction = null) {
			NormaSuscrita? normaSuscrita = await normaSuscritaBcp.Obtener(idNormaSuscrita, validarVigencia: validarVigencia, validarSub: validarSub, validarIdNegocio: validarIdNegocio, transaction: transaction);
            if (normaSuscrita != null) {
                if (incluirTemplate) await IncluirTemplate(normaSuscrita, transaction);
                if (incluirPeriodicidad) await IncluirTipoPeriodicidad(normaSuscrita, transaction);
                if (incluirCategoria) await IncluirCategoriaNorma(normaSuscrita, transaction);
                if (incluirCargo) await IncluirCargo(normaSuscrita, transaction);
                if (incluirFiscalizadores) await IncluirFiscalizadores(normaSuscrita, transaction);
                if (incluirNotificaciones) await IncluirNotificaciones(normaSuscrita, transaction);
				if (incluirHistorialVencimientos) await IncluirHistorialVencimientos(normaSuscrita, transaction);
				if (incluirProcesosNotificaciones) await IncluirProcesosNotificaciones(normaSuscrita, transaction);
			}
            return normaSuscrita;
		}

		public async Task<List<NormaSuscrita>> ObtenerPorSubYNegocio(string sub, long idNegocio, bool filtrarVigentes = false, bool incluirTemplates = false, bool incluirPeriodicidades = false, bool incluirCategorias = false, bool incluirCargos = false, bool incluirHistorialVencimientos = false, NpgsqlTransaction? transaction = null) {
			List<NormaSuscrita> normasSuscritas = await normaSuscritaBcp.ObtenerPorSubYNegocio(sub, idNegocio, filtrarVigentes: filtrarVigentes, transaction);
			if (incluirTemplates) await IncluirTemplate(normasSuscritas, transaction);
			if (incluirPeriodicidades) await IncluirTipoPeriodicidad(normasSuscritas, transaction);
			if (incluirCategorias) await IncluirCategoriaNorma(normasSuscritas, transaction);
			if (incluirCargos) await IncluirCargo(normasSuscritas, transaction);
			if (incluirHistorialVencimientos) await IncluirHistorialVencimientos(normasSuscritas, transaction);
			return normasSuscritas;
		}

		public async Task<NormaSuscrita> ObtenerIncluyendoProximoVencimiento(long idNormaSuscrita, string sub, NpgsqlTransaction? transaction = null) {
			NormaSuscrita obligacion = (await Obtener(
				idNormaSuscrita,
				validarVigencia: true,
				validarSub: sub,
				incluirTemplate: true,
				incluirPeriodicidad: true,
				incluirCategoria: true,
				incluirCargo: true,
				incluirFiscalizadores: true,
				incluirNotificaciones: true,
				incluirHistorialVencimientos: true
			))!;

			HistorialNormaSuscrita? proximoVencimiento = historialNormaSuscritaBcp.FiltrarUltimoVencimiento(obligacion.HistorialesNormaSuscrita ?? []);

			obligacion.HistorialesNormaSuscrita = [];
			if (proximoVencimiento != null) obligacion.HistorialesNormaSuscrita.Add(proximoVencimiento);
			
			return obligacion;
		}
		
        public async Task<(HistorialNormaSuscrita, bool tienePlanEmpresa)> ObtenerVencimientoConDocumentosYPlan(long? idNormaSuscrita, long idHistorialNormaSuscrita, string? sub, NpgsqlTransaction? transaction = null) {
			HistorialNormaSuscrita vencimiento = (await historialNormaSuscritaBcp.Obtener(idHistorialNormaSuscrita, validarVigencia: true, validarIdNormaSuscrita: idNormaSuscrita, transaction: transaction))!;

			vencimiento.NormaSuscrita = await Obtener(
				vencimiento.IdNormaSuscrita, 
				validarSub: sub, 
				incluirTemplate: true, 
				incluirPeriodicidad: true, 
				incluirCategoria: true, 
				incluirFiscalizadores: true, 
				incluirCargo: true,
				transaction: transaction
			) ?? throw new ErrorValidacion(TipoErrorValidacion.NoExiste, "La obligación no existe", "La obligación es inválida.");
			
			if (!historialNormaSuscritaBcp.EstaCompletada(vencimiento) && !normaSuscritaBcp.EstaVigente(vencimiento.NormaSuscrita)) {
				throw new ErrorValidacion(TipoErrorValidacion.EstadoNoValido, "La obligación no está vigente y el vencimiento no está completado", "La obligación es inválida.");
			}

			vencimiento.NormaSuscrita.Negocio = await negocioBcp.Obtener(vencimiento.NormaSuscrita.IdNegocio, validarVigencia: true, validarSub: sub, transaction: transaction);
			vencimiento.DocumentosAdjuntos = await documentoAdjuntoBcp.ObtenerPorVencimiento(vencimiento.Id, filtrarVigentes: true, filtrarRecepcionados: true, transaction: transaction);
			return (vencimiento, await suscripcionBcp.ConsultaTienePlanEmpresa(vencimiento.NormaSuscrita.Sub, transaction));
        }
		 
		public async Task<(HistorialNormaSuscrita, bool tienePlanEmpresa)> ObtenerVencimientoConDocumentosYPlan(string codigoAcceso, NpgsqlTransaction? transaction = null) {
			HistorialNotificacion historialNotificacion = await historialNotificacionBcp.ObtenerPorCodigoAccesoValidandoVigencia(codigoAcceso, transaction);
			return await ObtenerVencimientoConDocumentosYPlan(null, historialNotificacion.IdHistorialNormaSuscrita, null, transaction);
		}

		public async Task<(List<ProcesoNotificacion> procesosProgramados, List<ProcesoNotificacion> procesosDesprogramados)> ActualizarProgramacionProcesosNormaSuscrita(long idNormaSuscrita, NpgsqlTransaction? transaction = null) {
			List<ProcesoNotificacion> procesosProgramados = [];
			List<ProcesoNotificacion> procesosDesprogramados = [];
			try {
				NormaSuscrita? normaSuscrita = await Obtener(idNormaSuscrita, incluirTemplate: true, incluirProcesosNotificaciones: true, transaction: transaction) ?? throw new InvalidOperationException("Norma suscrita inválida");
				List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> cronsDeseados = [];
				List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> frecuenciasDiasDeseadas = [];

                // Si la norma suscrita está activada, se obtienen los crons y frecuencias deseados...
                if (normaSuscrita.Activado) {
					long idTipoPeriodicidad = (normaSuscrita.IdTipoPeriodicidad ?? normaSuscrita.TemplateNorma?.IdTipoPeriodicidad) ?? throw new InvalidOperationException("Obligación sin tipo de periodicidad asociada");
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

                (List<ProcesoNotificacion> cronsProgramados, List<ProcesoNotificacion> cronsDesprogramados) = await normaSuscritaBcp.ActualizarProcesosCronProgramados(normaSuscrita, cronsDeseados, transaction);
                procesosProgramados.AddRange(cronsProgramados);
                procesosDesprogramados.AddRange(cronsDesprogramados);

				(List<ProcesoNotificacion> frecuenciasDiasProgramados, List<ProcesoNotificacion> frecuenciasDiasDesprogramadas) = await normaSuscritaBcp.ActualizarProcesosFrecuenciaDiasProgramados(normaSuscrita, frecuenciasDiasDeseadas, transaction);
                procesosProgramados.AddRange(frecuenciasDiasProgramados);
                procesosDesprogramados.AddRange(frecuenciasDiasDesprogramadas);

				return (procesosProgramados, procesosDesprogramados);
			} catch {
				await ReversarProcesosProgramadosDesprogramados(procesosProgramados, procesosDesprogramados);
				throw;
			}
		}

		public async Task ReversarProcesosProgramadosDesprogramados(List<ProcesoNotificacion> procesosProgramados, List<ProcesoNotificacion> procesosDesprogramados) {
			await normaSuscritaBcp.ReversarProcesosProgramadosDesprogramados(procesosProgramados, procesosDesprogramados);
		}

		public async Task<(List<ProcesoNotificacion> procesosProgramados, List<ProcesoNotificacion> procesosDesprogramados)> EliminarNormaSuscrita(NormaSuscrita normaSuscrita, IDatabaseTransaction transaction) {
			List<ProcesoNotificacion> procesosProgramados = [];
			List<ProcesoNotificacion> procesosDesprogramados = [];

			if (normaSuscrita.Vigencia) {
				await normaSuscritaBcp.Eliminar(normaSuscrita, transaction!.NpgsqlTransaction());

				(procesosProgramados, procesosDesprogramados) = await ActualizarProgramacionProcesosNormaSuscrita(normaSuscrita.Id, transaction!.NpgsqlTransaction());

				await fiscalizadorNormaSuscritaBcp.EliminarPorNormaSuscrita(normaSuscrita.Id, transaction!.NpgsqlTransaction());
				await notificacionNormaSuscritaBcp.EliminarPorNormaSuscrita(normaSuscrita.Id, transaction!.NpgsqlTransaction());
				await historialNormaSuscritaUseCase.EliminarPorNormaSuscrita(normaSuscrita.Id, false, transaction!.NpgsqlTransaction());
			}

			return (procesosProgramados, procesosDesprogramados);
		}

        public async Task<(List<ProcesoNotificacion> procesosProgramados, List<ProcesoNotificacion> procesosDesprogramados)> EliminarNormaValidandoPertenencia(string sub, long idNormaSuscrita, IDatabaseTransaction? transaction = null) {
            bool ownsTransaction = transaction == null;
            IDatabaseConnection? connection = null;

			List<ProcesoNotificacion> procesosProgramados = [];
			List<ProcesoNotificacion> procesosDesprogramados = [];
			try {
                if (ownsTransaction) {
                    connection = await connectionHelper.ObtenerConexionWrapper();
                    transaction = await connection.BeginTransactionAsync();
                }

				NormaSuscrita? obligacion = await normaSuscritaBcp.Obtener(idNormaSuscrita, filtrarVigente: true, validarSub: sub, validarEditable: true, transaction: transaction!.NpgsqlTransaction());
                if (obligacion != null) {
					(procesosProgramados, procesosDesprogramados) = await EliminarNormaSuscrita(obligacion, transaction);
                }

                if (ownsTransaction) {
                    await transaction!.CommitAsync();
                }

				return (procesosProgramados, procesosDesprogramados);
            } catch {
                if (ownsTransaction && transaction != null) {
                    await transaction.RollbackAsync();
					await ReversarProcesosProgramadosDesprogramados(procesosProgramados, procesosDesprogramados);
				}
                throw;
            } finally {
                if (ownsTransaction) {
                    if (transaction != null) await transaction.DisposeAsync();
                    if (connection != null) await connection.DisposeAsync();
                }
            }
        }

        public async Task<(NormaSuscrita, List<ProcesoNotificacion> procesosProgramados, List<ProcesoNotificacion> procesosDesprogramados)> CrearNormaSuscrita(string sub, long idNegocio, string nombre, string? descripcion, string? multa, long? idTipoPeriodicidad, long? idCategoriaNorma, long? idCargo, bool activado, DateTime? proximoVencimiento, HashSet<long> idFiscalizadores, HashSet<(long IdTipoUnidadTiempo, int CantAntelacion)> antelaciones, IDatabaseTransaction? transaction = null) {
            bool ownsTransaction = transaction == null;
            IDatabaseConnection? connection = null;

			List<ProcesoNotificacion> procesosProgramados = [];
			List<ProcesoNotificacion> procesosDesprogramados = [];
			try {
                if (ownsTransaction) {
                    connection = await connectionHelper.ObtenerConexionWrapper();
                    transaction = await connection.BeginTransactionAsync();
                }

                bool tienePlanEmpresa = await suscripcionBcp.ConsultaTienePlanEmpresa(sub, transaction!.NpgsqlTransaction());
                if (!tienePlanEmpresa && idCargo != null) throw new ErrorValidacion(TipoErrorValidacion.RestringidoPorPlan, "Tu plan no permite asignar un cargo responsable a la obligación.");

                Dictionary<long, TipoFiscalizador> tiposFiscalizadores = (await tipoFiscalizadorBcp.ValidarTodosVigentes(idFiscalizadores, transaction!.NpgsqlTransaction())).ToDictionary(f => f.Id, f => f);
                Dictionary<long, TipoUnidadTiempo> tiposUnidadesTiempo = (await tipoUnidadTiempoBcp.ValidarTodosVigentes([.. antelaciones.Select(a => a.IdTipoUnidadTiempo)], transaction!.NpgsqlTransaction())).ToDictionary(u => u.Id, u => u);
                if (antelaciones.Any(a => a.CantAntelacion <= 0)) throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "Una notificación con cantidad antelación inválido.");
                if (activado) {
                    if (proximoVencimiento == null) throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "Debe incluir la fecha de próximo vencimiento.");
                    if (proximoVencimiento!.Value <= dateTimeProvider.UtcNow) throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "El próximo vencimiento debe ser una fecha futura.");
                }

				_ = await negocioBcp.Obtener(idNegocio, validarVigencia: true, validarSub: sub, transaction: transaction!.NpgsqlTransaction())!;

                TipoPeriodicidad? periodicidad = null;
                if (idTipoPeriodicidad != null) periodicidad = await tipoPeriodicidadBcp.ObtenerValidandoVigencia(idTipoPeriodicidad, transaction!.NpgsqlTransaction());

                CategoriaNorma? categoriaNorma = null;
                if (idCategoriaNorma != null) categoriaNorma = await categoriaNormaBcp.ObtenerValidandoVigencia(idCategoriaNorma, transaction!.NpgsqlTransaction());

                Cargo? cargo = null;
                if (idCargo != null) cargo = await cargoBcp.Obtener(idCargo.Value, validarVigencia: true, validarSub: sub, validarIdNegocio: idNegocio, transaction: transaction!.NpgsqlTransaction());

                NormaSuscrita obligacion = await normaSuscritaBcp.CrearObligacionUsuario(sub, idNegocio, nombre, descripcion, multa, idTipoPeriodicidad, idCategoriaNorma, idCargo, activado, transaction!.NpgsqlTransaction());
                obligacion.TipoPeriodicidad = periodicidad;
                obligacion.CategoriaNorma = categoriaNorma;
                obligacion.Cargo = cargo;

				obligacion.FiscalizadoresNormaSuscrita = await fiscalizadorNormaSuscritaBcp.ActualizarPorNormaSuscrita(obligacion.Id, idFiscalizadores, transaction!.NpgsqlTransaction());
				obligacion.FiscalizadoresNormaSuscrita = [.. obligacion.FiscalizadoresNormaSuscrita.Select(f => {
                    f.TipoFiscalizador = tiposFiscalizadores[f.IdTipoFiscalizador];
                    return f;
                })];

				obligacion.NotificacionesNormaSuscrita = await notificacionNormaSuscritaBcp.ActualizarPorNormaSuscrita(obligacion.Id, antelaciones, transaction!.NpgsqlTransaction());
				obligacion.NotificacionesNormaSuscrita = [.. obligacion.NotificacionesNormaSuscrita.Select(f => {
                    f.TipoUnidadTiempo = tiposUnidadesTiempo[f.IdTipoUnidadTiempoAntelacion];
                    return f;
                })];

                obligacion.HistorialesNormaSuscrita = [];
				if (activado && proximoVencimiento != null) obligacion.HistorialesNormaSuscrita.Add(await historialNormaSuscritaBcp.Crear(obligacion.Id, proximoVencimiento.Value, transaction!.NpgsqlTransaction()));
				
				(procesosProgramados, procesosDesprogramados) = await ActualizarProgramacionProcesosNormaSuscrita(obligacion.Id, transaction!.NpgsqlTransaction());

                if (ownsTransaction) {
                    await transaction!.CommitAsync();
                }

                return (obligacion, procesosProgramados, procesosDesprogramados);
            } catch {
                if (ownsTransaction && transaction != null) {
                    await transaction.RollbackAsync();
					await ReversarProcesosProgramadosDesprogramados(procesosProgramados, procesosDesprogramados);
				}
                throw;
            } finally {
                if (ownsTransaction) {
                    if (transaction != null) await transaction.DisposeAsync();
                    if (connection != null) await connection.DisposeAsync();
                }
            }
        }

		public async Task<(NormaSuscrita, List<ProcesoNotificacion> procesosProgramados, List<ProcesoNotificacion> procesosDesprogramados)> ActualizarNormaSuscrita(string sub, long id, long idNegocio, string? nombre, string? descripcion, string? multa, long? idTipoPeriodicidad, long? idCategoriaNorma, long? idCargo, bool activado, DateTime? proximoVencimiento, HashSet<long> idFiscalizadores, HashSet<(long IdTipoUnidadTiempo, int CantAntelacion)> antelaciones, IDatabaseTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			IDatabaseConnection? connection = null;

			List<ProcesoNotificacion> procesosProgramados = [];
			List<ProcesoNotificacion> procesosDesprogramados = [];
			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexionWrapper();
					transaction = await connection.BeginTransactionAsync();
				}

				NormaSuscrita obligacion = (await Obtener(
					id, 
					validarVigencia: true, 
					validarSub: sub, 
					validarIdNegocio: idNegocio, 
					incluirTemplate: true, 
					incluirFiscalizadores: true, 
					incluirNotificaciones: true, 
					transaction: transaction!.NpgsqlTransaction()
				))!;

				nombre = string.IsNullOrWhiteSpace(nombre) ? null : nombre?.Trim();
				descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion?.Trim();
				multa = string.IsNullOrWhiteSpace(multa) ? null : multa?.Trim();

				// Se setean en null atributos que sean igual a template norma...
				if (obligacion.TemplateNorma != null) {
					if (obligacion.TemplateNorma.Nombre == nombre) nombre = null;
					if (obligacion.TemplateNorma.Descripcion == descripcion) descripcion = null;
					if (obligacion.TemplateNorma.Multa == multa) multa = null;
					if (obligacion.TemplateNorma.IdTipoPeriodicidad == idTipoPeriodicidad) idTipoPeriodicidad = null;
					if (obligacion.TemplateNorma.IdCategoriaNorma == idCategoriaNorma) idCategoriaNorma = null;
					if ((obligacion.TemplateNorma.TemplateNormaFiscalizadores ?? []).Select(f => f.IdTipoFiscalizador).ToHashSet().SetEquals(idFiscalizadores)) idFiscalizadores = [];
					if ((obligacion.TemplateNorma.TemplateNormaNotificaciones ?? []).Select(n => (n.IdTipoUnidadTiempoAntelacion, n.CantAntelacion)).ToHashSet().SetEquals(antelaciones)) antelaciones = [];
				}

				bool tienePlanEmpresa = await suscripcionBcp.ConsultaTienePlanEmpresa(sub, transaction!.NpgsqlTransaction());
				if (!tienePlanEmpresa && idCargo != null) throw new ErrorValidacion(TipoErrorValidacion.RestringidoPorPlan, "Tu plan no permite asignar un cargo responsable a la obligación.");

				Dictionary<long, TipoFiscalizador> tiposFiscalizadores = (await tipoFiscalizadorBcp.ValidarTodosVigentes(idFiscalizadores, transaction!.NpgsqlTransaction())).ToDictionary(f => f.Id, f => f);
				Dictionary<long, TipoUnidadTiempo> tiposUnidadesTiempo = (await tipoUnidadTiempoBcp.ValidarTodosVigentes([.. antelaciones.Select(a => a.IdTipoUnidadTiempo)], transaction!.NpgsqlTransaction())).ToDictionary(u => u.Id, u => u);
				if (antelaciones.Any(a => a.CantAntelacion <= 0)) throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "Una notificación con cantidad antelación inválido.");

				List<HistorialNormaSuscrita> vencimientos = await historialNormaSuscritaBcp.ObtenerPorNormaSuscrita(obligacion.Id, filtrarVigente: true, transaction: transaction!.NpgsqlTransaction());
				HistorialNormaSuscrita? proximoVencimientoExistente = historialNormaSuscritaBcp.FiltrarUltimoVencimiento(vencimientos);

				TipoPeriodicidad? periodicidad = null;
				if (idTipoPeriodicidad != null) periodicidad = await tipoPeriodicidadBcp.ObtenerValidandoVigencia(idTipoPeriodicidad, transaction!.NpgsqlTransaction());

				CategoriaNorma? categoriaNorma = null;
				if (idCategoriaNorma != null) categoriaNorma = await categoriaNormaBcp.ObtenerValidandoVigencia(idCategoriaNorma, transaction!.NpgsqlTransaction());

				Cargo? cargo = null;
				if (idCargo != null) cargo = await cargoBcp.Obtener(idCargo.Value, validarVigencia: true, validarSub: sub, validarIdNegocio: idNegocio, transaction: transaction!.NpgsqlTransaction());

				if (activado) {
					if (proximoVencimiento == null) throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "Debe incluir la fecha de próximo vencimiento.");

					// Se modifica próximo vencimiento si es una fecha pasada y según periodicidad es posible calcular un próximo vencimiento...
					if (proximoVencimiento.Value <= dateTimeProvider.UtcNow) {
						if (periodicidad != null && (periodicidad.DeltaDias != null || periodicidad.DeltaMeses != null || periodicidad.DeltaAnnos != null)) {
							proximoVencimiento = historialNormaSuscritaUseCase.CalcularVencimientoFuturo(proximoVencimiento.Value, periodicidad);
						}
					} 

					if (proximoVencimientoExistente?.FechaVencimiento != proximoVencimiento && proximoVencimiento!.Value <= dateTimeProvider.UtcNow) throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "El próximo vencimiento debe ser una fecha futura.");
				}

				_ = await negocioBcp.Obtener(idNegocio, validarVigencia: true, validarSub: sub, transaction: transaction!.NpgsqlTransaction())!;

				if (obligacion.Nombre != nombre || obligacion.Descripcion != descripcion || obligacion.Multa != multa ||
					obligacion.IdTipoPeriodicidad != idTipoPeriodicidad || obligacion.IdCategoriaNorma != idCategoriaNorma ||
					obligacion.IdCargo != idCargo) {

					obligacion.Nombre = nombre;
					obligacion.Descripcion = descripcion;
					obligacion.Multa = multa;
					obligacion.IdTipoPeriodicidad = idTipoPeriodicidad;
					obligacion.IdCategoriaNorma = idCategoriaNorma;
					obligacion.IdCargo = idCargo;

					await normaSuscritaBcp.Actualizar(obligacion, transaction!.NpgsqlTransaction());
				}

				obligacion.TipoPeriodicidad = periodicidad;
				obligacion.CategoriaNorma = categoriaNorma;
				obligacion.Cargo = cargo;

				if (obligacion.Activado != activado) {
					if (activado) {
						await normaSuscritaBcp.Activar(obligacion, transaction!.NpgsqlTransaction());
					} else {
						await normaSuscritaBcp.Desactivar(obligacion, transaction!.NpgsqlTransaction());
					}
				}

				obligacion.FiscalizadoresNormaSuscrita = await fiscalizadorNormaSuscritaBcp.ActualizarPorNormaSuscrita(obligacion.Id, idFiscalizadores, transaction!.NpgsqlTransaction());
				obligacion.FiscalizadoresNormaSuscrita = [.. obligacion.FiscalizadoresNormaSuscrita.Select(f => {
					f.TipoFiscalizador = tiposFiscalizadores[f.IdTipoFiscalizador];
					return f;
				})];

				obligacion.NotificacionesNormaSuscrita = await notificacionNormaSuscritaBcp.ActualizarPorNormaSuscrita(obligacion.Id, antelaciones, transaction!.NpgsqlTransaction());
				obligacion.NotificacionesNormaSuscrita = [.. obligacion.NotificacionesNormaSuscrita.Select(f => {
					f.TipoUnidadTiempo = tiposUnidadesTiempo[f.IdTipoUnidadTiempoAntelacion];
					return f;
				})];

				obligacion.HistorialesNormaSuscrita = [];
				if (activado) {
					if (proximoVencimientoExistente?.FechaVencimiento != proximoVencimiento) {
						await historialNormaSuscritaUseCase.EliminarPorNormaSuscrita(obligacion.Id, true, transaction!.NpgsqlTransaction());
						obligacion.HistorialesNormaSuscrita.Add(await historialNormaSuscritaBcp.Crear(obligacion.Id, proximoVencimiento!.Value, transaction!.NpgsqlTransaction()));
					} else {
						obligacion.HistorialesNormaSuscrita.Add(proximoVencimientoExistente!);
					}
				} else {
					await historialNormaSuscritaUseCase.EliminarPorNormaSuscrita(obligacion.Id, false, transaction!.NpgsqlTransaction());
				}

				(procesosProgramados, procesosDesprogramados) = await ActualizarProgramacionProcesosNormaSuscrita(obligacion.Id, transaction!.NpgsqlTransaction());

				if (ownsTransaction) {
					await transaction!.CommitAsync();
				}

				return (obligacion, procesosProgramados, procesosDesprogramados);
			} catch {
				if (ownsTransaction && transaction != null) {
					await transaction.RollbackAsync();
					await ReversarProcesosProgramadosDesprogramados(procesosProgramados, procesosDesprogramados);
				}
				throw;
			} finally {
				if (ownsTransaction) {
					if (transaction != null) await transaction.DisposeAsync();
					if (connection != null) await connection.DisposeAsync();
				}
			}
		}

		public async Task<HistorialNormaSuscrita> CompletarNormaValidandoPertenencia(string sub, long idNormaSuscrita, long idHistorialNormaSuscrita, IDatabaseTransaction? transaction = null) {
            bool ownsTransaction = transaction == null;
            IDatabaseConnection? connection = null;
            try {
                if (ownsTransaction) {
                    connection = await connectionHelper.ObtenerConexionWrapper();
                    transaction = await connection.BeginTransactionAsync();
                }

				NormaSuscrita obligacion = (await normaSuscritaBcp.Obtener(idNormaSuscrita, validarVigencia: true, validarSub: sub, transaction: transaction!.NpgsqlTransaction()))!;
				HistorialNormaSuscrita vencimiento = (await historialNormaSuscritaBcp.Obtener(idHistorialNormaSuscrita, validarVigencia: true, validarIdNormaSuscrita: idNormaSuscrita, transaction: transaction!.NpgsqlTransaction()))!;

				vencimiento.FechaCompletitud = await historialNormaSuscritaUseCase.CompletarHistorialNormaSuscrita(vencimiento, transaction!.NpgsqlTransaction());

                if (ownsTransaction) {
                    await transaction!.CommitAsync();
                }

                return vencimiento;
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

		public async Task<HistorialNormaSuscrita> CompletarNormaPorCodigoAcceso(string codigoAcceso, IDatabaseTransaction? transaction = null) {
            bool ownsTransaction = transaction == null;
            IDatabaseConnection? connection = null;
            try {
                if (ownsTransaction) {
                    connection = await connectionHelper.ObtenerConexionWrapper();
                    transaction = await connection.BeginTransactionAsync();
                }

                HistorialNotificacion historialNotificacion = await historialNotificacionBcp.ObtenerPorCodigoAccesoValidandoVigencia(codigoAcceso, transaction!.NpgsqlTransaction());
				HistorialNormaSuscrita vencimiento = (await historialNormaSuscritaBcp.Obtener(historialNotificacion.IdHistorialNormaSuscrita, validarVigencia: true, transaction: transaction!.NpgsqlTransaction()))!;
				NormaSuscrita obligacion = (await normaSuscritaBcp.Obtener(vencimiento.IdNormaSuscrita, validarVigencia: true, transaction: transaction!.NpgsqlTransaction()))!;

				vencimiento.FechaCompletitud = await historialNormaSuscritaUseCase.CompletarHistorialNormaSuscrita(vencimiento, transaction!.NpgsqlTransaction());

                if (ownsTransaction) {
                    await transaction!.CommitAsync();
                }

				return vencimiento;
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

		public async Task<(NormaSuscrita, List<ProcesoNotificacion> procesosProgramados, List<ProcesoNotificacion> procesosDesprogramados)> DesactivarNormaSuscrita(long id, string sub, IDatabaseTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			IDatabaseConnection? connection = null;

			List<ProcesoNotificacion> procesosProgramados = [];
			List<ProcesoNotificacion> procesosDesprogramados = [];
			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexionWrapper();
					transaction = await connection.BeginTransactionAsync();
				}

				NormaSuscrita obligacion = (await Obtener(id, validarVigencia: true, validarSub: sub, incluirTemplate: true, transaction: transaction!.NpgsqlTransaction()))!;
				if (normaSuscritaBcp.EstaActiva(obligacion)) {
					await normaSuscritaBcp.Desactivar(obligacion, transaction!.NpgsqlTransaction()); 
					await historialNormaSuscritaUseCase.EliminarPorNormaSuscrita(obligacion.Id, false, transaction!.NpgsqlTransaction());
					obligacion.HistorialesNormaSuscrita = [];
					(procesosProgramados, procesosDesprogramados) = await ActualizarProgramacionProcesosNormaSuscrita(obligacion.Id, transaction!.NpgsqlTransaction());
				}

				if (ownsTransaction) {
					await transaction!.CommitAsync();
				}

				return (obligacion, procesosProgramados, procesosDesprogramados);
			} catch {
				if (ownsTransaction && transaction != null) {
					await transaction.RollbackAsync();
					await ReversarProcesosProgramadosDesprogramados(procesosProgramados, procesosDesprogramados);
				}
				throw;
			} finally {
				if (ownsTransaction) {
					if (transaction != null) await transaction.DisposeAsync();
					if (connection != null) await connection.DisposeAsync();
				}
			}
		}

		public async Task<(NormaSuscrita, List<ProcesoNotificacion> procesosProgramados, List<ProcesoNotificacion> procesosDesprogramados)> ActivarNormaSuscrita(long id, string sub, DateTime proximoVencimiento, IDatabaseTransaction? transaction = null) {
			bool ownsTransaction = transaction == null;
			IDatabaseConnection? connection = null;

			List<ProcesoNotificacion> procesosProgramados = [];
			List<ProcesoNotificacion> procesosDesprogramados = [];
			try {
				if (ownsTransaction) {
					connection = await connectionHelper.ObtenerConexionWrapper();
					transaction = await connection.BeginTransactionAsync();
				}

				NormaSuscrita obligacion = (await Obtener(id, validarVigencia: true, validarSub: sub, incluirTemplate: true, transaction: transaction!.NpgsqlTransaction()))!;
				if (!normaSuscritaBcp.EstaActiva(obligacion)) {
					TipoPeriodicidad periodicidad = await tipoPeriodicidadBcp.ObtenerValidandoVigencia(obligacion.IdTipoPeriodicidad ?? obligacion.TemplateNorma?.IdTipoPeriodicidad, transaction!.NpgsqlTransaction());

					// Se modifica próximo vencimiento si es una fecha pasada y según periodicidad es posible calcular un próximo vencimiento...
					if (proximoVencimiento <= dateTimeProvider.UtcNow) {
						if (periodicidad.DeltaDias != null || periodicidad.DeltaMeses != null || periodicidad.DeltaAnnos != null) {
							proximoVencimiento = historialNormaSuscritaUseCase.CalcularVencimientoFuturo(proximoVencimiento, periodicidad);
						}
					}

					if (proximoVencimiento <= dateTimeProvider.UtcNow) {
						throw new ErrorValidacion(TipoErrorValidacion.ValorNoValido, "El próximo vencimiento debe ser una fecha futura.");
					}

					await normaSuscritaBcp.Activar(obligacion, transaction!.NpgsqlTransaction());					
					obligacion.HistorialesNormaSuscrita = [];
					obligacion.HistorialesNormaSuscrita.Add(await historialNormaSuscritaBcp.Crear(obligacion.Id, proximoVencimiento, transaction!.NpgsqlTransaction()));
					(procesosProgramados, procesosDesprogramados) = await ActualizarProgramacionProcesosNormaSuscrita(obligacion.Id, transaction!.NpgsqlTransaction());
				}

				if (ownsTransaction) {
					await transaction!.CommitAsync();
				}

				return (obligacion, procesosProgramados, procesosDesprogramados);
			} catch {
				if (ownsTransaction && transaction != null) {
					await transaction.RollbackAsync();
					await ReversarProcesosProgramadosDesprogramados(procesosProgramados, procesosDesprogramados);
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
