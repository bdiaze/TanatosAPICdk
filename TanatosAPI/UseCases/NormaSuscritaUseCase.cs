using Microsoft.AspNetCore.SignalR;
using Npgsql;
using TanatosAPI.Entities.Models;
using TanatosAPI.Exceptions;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Repositories;

namespace TanatosAPI.UseCases {
	public class NormaSuscritaUseCase(IDatabaseConnectionHelper connectionHelper, IDateTimeProvider dateTimeProvider, HistorialNormaSuscritaUseCase historialNormaSuscritaUseCase, NotificacionNormaSuscritaUseCase notificacionNormaSuscritaUseCase, INormaSuscritaBcp normaSuscritaBcp, IHistorialNormaSuscritaBcp historialNormaSuscritaBcp, IHistorialNotificacionBcp historialNotificacionBcp, IFiscalizadorNormaSuscritaBcp fiscalizadorNormaSuscritaBcp, INotificacionNormaSuscritaBcp notificacionNormaSuscritaBcp, ITemplateBcp templateBcp, ITemplateNormaBcp templateNormaBcp, ITemplateNormaNotificacionBcp templateNormaNotificacionBcp, ITemplateNormaFiscalizadorBcp templateNormaFiscalizadorBcp, ITipoPeriodicidadBcp tipoPeriodicidadBcp, ICategoriaNormaBcp categoriaNormaBcp, ITipoFiscalizadorBcp tipoFiscalizadorBcp, ITipoUnidadTiempoBcp tipoUnidadTiempoBcp, ICargoBcp cargoBcp, INegocioBcp negocioBcp, ISuscripcionBcp suscripcionBcp) {
		public async Task<NormaSuscrita?> ObtenerConTemplate(long idNormaSuscrita, NpgsqlTransaction? transaction = null) {
			NormaSuscrita? normaSuscrita = await normaSuscritaBcp.ObtenerPorId(idNormaSuscrita, transaction);
			if (normaSuscrita?.IdTemplate != null && normaSuscrita?.IdNorma != null) {
                TemplateNorma? templateNorma = await templateNormaBcp.ObtenerPorTemplateNorma(normaSuscrita.IdTemplate.Value, normaSuscrita.IdNorma.Value, transaction);
                Template? template = templateNorma == null ? null : await templateBcp.ObtenerSoloVigente(templateNorma.IdTemplate, transaction);
                if (templateBcp.EstaVigente(template)) {
                    templateNorma!.Template = template;
					normaSuscrita.TemplateNorma = templateNorma;
				}
			}
			return normaSuscrita;
		}

		public async Task<NormaSuscrita> ObtenerConTemplateValidandoVigenciaYPertenencia(long idNormaSuscrita, string sub, NpgsqlTransaction? transaction = null) {
			NormaSuscrita o = await normaSuscritaBcp.ObtenerValidandoVigenciaYPertenencia(idNormaSuscrita, sub, transaction);

			if (o.IdTemplate != null && o.IdNorma != null) {
				TemplateNorma? templateNorma = await templateNormaBcp.ObtenerPorTemplateNorma(o.IdTemplate.Value, o.IdNorma.Value, transaction);
				Template? template = templateNorma == null ? null : await templateBcp.ObtenerSoloVigente(templateNorma.IdTemplate, transaction);
				if (templateBcp.EstaVigente(template)) {
					templateNorma!.Template = template;
					o.TemplateNorma = templateNorma;
				}
			}
			return o;
		}

		public async Task<NormaSuscrita> ObtenerConTemplateYTiposValidandoVigenciaYPertenencia(long idNormaSuscrita, string sub, NpgsqlTransaction? transaction = null) {
            NormaSuscrita o = await ObtenerConTemplateValidandoVigenciaYPertenencia(idNormaSuscrita, sub, transaction);

            if ((o.IdTipoPeriodicidad ?? o.TemplateNorma?.IdTipoPeriodicidad) != null) {
				Dictionary<long, TipoPeriodicidad> periodicidades = (await tipoPeriodicidadBcp.ObtenerVigentes(transaction)).ToDictionary(p => p.Id, p => p);
                o.TipoPeriodicidad = o.IdTipoPeriodicidad != null && periodicidades.TryGetValue(o.IdTipoPeriodicidad.Value, out TipoPeriodicidad? tp) ? tp : null;
                o.IdTipoPeriodicidad = o.TipoPeriodicidad?.Id;

                if (o.TemplateNorma != null) {
					o.TemplateNorma.TipoPeriodicidad = o.TemplateNorma.IdTipoPeriodicidad != null && periodicidades.TryGetValue(o.TemplateNorma.IdTipoPeriodicidad.Value, out TipoPeriodicidad? tpt) ? tpt : null;
					o.TemplateNorma.IdTipoPeriodicidad = o.TemplateNorma.TipoPeriodicidad?.Id;
				}
			}

            if ((o.IdCategoriaNorma ?? o.TemplateNorma?.IdCategoriaNorma) != null) {
				Dictionary<long, CategoriaNorma> categorias = (await categoriaNormaBcp.ObtenerVigentes(transaction)).ToDictionary(p => p.Id, p => p);
				o.CategoriaNorma = o.IdCategoriaNorma != null && categorias.TryGetValue(o.IdCategoriaNorma.Value, out CategoriaNorma? cn) ? cn : null;
				o.IdCategoriaNorma = o.CategoriaNorma?.Id;

                if (o.TemplateNorma != null) {
					o.TemplateNorma.CategoriaNorma = categorias.TryGetValue(o.TemplateNorma.IdCategoriaNorma, out CategoriaNorma? cnt) ? cnt : null;
					o.TemplateNorma.IdCategoriaNorma = o.TemplateNorma.CategoriaNorma?.Id ?? o.TemplateNorma.IdCategoriaNorma;
				}
			}

            if (o.IdCargo != null) {
                o.Cargo = await cargoBcp.ObtenerSoloVigente(o.IdCargo.Value, transaction);
                o.IdCargo = o.Cargo?.Id;
			}

			return o;
        }

        public async Task<NormaSuscrita> ObtenerConTemplateTiposFiscalizadoresYNotificacionesValidandoVigenciaYPertenencia(long idNormaSuscrita, string sub, NpgsqlTransaction? transaction = null) {
            NormaSuscrita o = await ObtenerConTemplateYTiposValidandoVigenciaYPertenencia(idNormaSuscrita, sub, transaction);

            o.FiscalizadoresNormaSuscrita = await fiscalizadorNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(o.Id, transaction);
            o.NotificacionesNormaSuscrita = await notificacionNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(o.Id, transaction);
            if (o.TemplateNorma != null) {
                o.TemplateNorma.TemplateNormaFiscalizadores = await templateNormaFiscalizadorBcp.ObtenerPorTemplateNorma(o.TemplateNorma.IdTemplate, o.TemplateNorma.IdNorma, transaction);
                o.TemplateNorma.TemplateNormaNotificaciones = await templateNormaNotificacionBcp.ObtenerPorTemplateNorma(o.TemplateNorma.IdTemplate, o.TemplateNorma.IdNorma, transaction);
            }

            if (o.FiscalizadoresNormaSuscrita.Count > 0 || o.TemplateNorma?.TemplateNormaFiscalizadores?.Count > 0) {
                Dictionary<long, TipoFiscalizador> fiscalizadores = (await tipoFiscalizadorBcp.ObtenerVigentes(transaction)).ToDictionary(p => p.Id, p => p);
                
                foreach (FiscalizadorNormaSuscrita f in o.FiscalizadoresNormaSuscrita.ToList()) {
                    f.TipoFiscalizador = fiscalizadores.TryGetValue(f.IdTipoFiscalizador, out TipoFiscalizador? tf) ? tf : null;
                    if (f.TipoFiscalizador == null) o.FiscalizadoresNormaSuscrita.Remove(f); 
                }

                foreach(TemplateNormaFiscalizador f in o.TemplateNorma?.TemplateNormaFiscalizadores?.ToList() ?? []) {
                    f.TipoFiscalizador = fiscalizadores.TryGetValue(f.IdTipoFiscalizador, out TipoFiscalizador? tf) ? tf : null;
					if (f.TipoFiscalizador == null) o.TemplateNorma!.TemplateNormaFiscalizadores!.Remove(f);
				}
            }

			if (o.NotificacionesNormaSuscrita.Count > 0 || o.TemplateNorma?.TemplateNormaNotificaciones?.Count > 0) {
				Dictionary<long, TipoUnidadTiempo> unidades = (await tipoUnidadTiempoBcp.ObtenerVigentes(transaction)).ToDictionary(p => p.Id, p => p);

				foreach (NotificacionNormaSuscrita f in o.NotificacionesNormaSuscrita.ToList()) {
					f.TipoUnidadTiempo = unidades.TryGetValue(f.IdTipoUnidadTiempoAntelacion, out TipoUnidadTiempo? ut) ? ut : null;
					if (f.TipoUnidadTiempo == null) o.NotificacionesNormaSuscrita.Remove(f);
				}

				foreach (TemplateNormaNotificacion f in o.TemplateNorma?.TemplateNormaNotificaciones?.ToList() ?? []) {
					f.TipoUnidadTiempoAntelacion = unidades.TryGetValue(f.IdTipoUnidadTiempoAntelacion, out TipoUnidadTiempo? ut) ? ut : null;
					if (f.TipoUnidadTiempoAntelacion == null) o.TemplateNorma!.TemplateNormaNotificaciones!.Remove(f);
				}
			}

			return o;
        }

		public async Task<NormaSuscrita> ObtenerConTemplateTiposFiscalizadoresNotificacionesYVencimientoValidandoVigenciaYPertenencia(long idNormaSuscrita, string sub, NpgsqlTransaction? transaction = null) {
            NormaSuscrita o = await ObtenerConTemplateTiposFiscalizadoresYNotificacionesValidandoVigenciaYPertenencia(idNormaSuscrita, sub, transaction);

			o.HistorialesNormaSuscrita = [];
			HistorialNormaSuscrita? ultimoVencimientoVigente = await historialNormaSuscritaBcp.ObtenerUltimoVigentePorNormaSuscrita(o.Id, transaction);
            if (ultimoVencimientoVigente != null) o.HistorialesNormaSuscrita.Add(ultimoVencimientoVigente);

            return o;
		}

		public async Task<List<NormaSuscrita>> ObtenerVigentesPorSubConTemplates(string sub, long idNegocio, NpgsqlTransaction? transaction = null) {
			List<NormaSuscrita> obligaciones = await normaSuscritaBcp.ObtenerVigentesPorSubYNegocio(sub, idNegocio, transaction);

			if (obligaciones.Count > 0) {
				List<Template> templates = await templateBcp.ObtenerVariosSoloVigentes([.. obligaciones.Where(o => o.IdTemplate != null).Select(o => o.IdTemplate!.Value)], transaction);

				if (templates.Count > 0) {
					Dictionary<(long idTemplate, long idNorma), TemplateNorma> templatesNormas = [];
					foreach (Template template in templates) {
						foreach (TemplateNorma templateNorma in await templateNormaBcp.ObtenerPorTemplate(template.Id, transaction)) {
							templateNorma.Template = template;
							templatesNormas[(templateNorma.IdTemplate, templateNorma.IdNorma)] = templateNorma;
						}
					}

					obligaciones = [.. obligaciones.Select(o => {
						o.TemplateNorma = o.IdTemplate != null && o.IdNorma != null && templatesNormas.TryGetValue((o.IdTemplate.Value, o.IdNorma.Value), out TemplateNorma? tn) ? tn : null;
						o.IdTemplate = o.TemplateNorma?.IdTemplate;
						o.IdNorma = o.TemplateNorma?.IdNorma;

						return o;
					})];
				}
			}

            return obligaciones;
		}

        public async Task<List<NormaSuscrita>> ObtenerVigentesPorSubConTemplatesYTipos(string sub, long idNegocio, NpgsqlTransaction? transaction = null) {
			List<NormaSuscrita> obligaciones = await ObtenerVigentesPorSubConTemplates(sub, idNegocio, transaction);
                        
            if (obligaciones.Count > 0) {
                Dictionary<long, TipoPeriodicidad> periodicidades = (await tipoPeriodicidadBcp.ObtenerVigentes(transaction)).ToDictionary(p => p.Id, p => p);
                Dictionary<long, CategoriaNorma> categorias = (await categoriaNormaBcp.ObtenerVigentes(transaction)).ToDictionary(p => p.Id, p => p);
				Dictionary<long, Cargo> cargos = (await cargoBcp.ObtenerVigentes(sub, idNegocio, transaction)).ToDictionary(p => p.Id, p => p);

				obligaciones = [.. obligaciones.Select(o => {
					o.TipoPeriodicidad = o.IdTipoPeriodicidad != null && periodicidades.TryGetValue(o.IdTipoPeriodicidad.Value, out TipoPeriodicidad? tp) ? tp : null;
					o.IdTipoPeriodicidad = o.TipoPeriodicidad?.Id;

					o.CategoriaNorma = o.IdCategoriaNorma != null && categorias.TryGetValue(o.IdCategoriaNorma.Value, out CategoriaNorma? cn) ? cn : null;
					o.IdCategoriaNorma = o.CategoriaNorma?.Id;

					o.Cargo = o.IdCargo != null && cargos.TryGetValue(o.IdCargo.Value, out Cargo? c) ? c : null;
					o.IdCargo = o.Cargo?.Id;

					if (o.TemplateNorma != null) {
						o.TemplateNorma.TipoPeriodicidad = o.TemplateNorma.IdTipoPeriodicidad != null && periodicidades.TryGetValue(o.TemplateNorma.IdTipoPeriodicidad.Value, out TipoPeriodicidad? tpt) ? tpt : null;
						o.TemplateNorma.IdTipoPeriodicidad = o.TemplateNorma.TipoPeriodicidad?.Id;

						o.TemplateNorma.CategoriaNorma = categorias.TryGetValue(o.TemplateNorma.IdCategoriaNorma, out CategoriaNorma? cnt) ? cnt : null;
					    o.TemplateNorma.IdCategoriaNorma = o.TemplateNorma.CategoriaNorma?.Id ?? o.TemplateNorma.IdCategoriaNorma;
					}

					return o;
				})];
            }

            return obligaciones;
		}
				
        public async Task<List<NormaSuscrita>> ObtenerVigentesPorSubConTemplatesTiposEHistorialVencimientos(string sub, long idNegocio, NpgsqlTransaction? transaction = null) {
            List<NormaSuscrita> obligaciones = await ObtenerVigentesPorSubConTemplatesYTipos(sub, idNegocio, transaction);

            obligaciones = [.. await Task.WhenAll(
                obligaciones.Select(async o => {
                    if (normaSuscritaBcp.EstaActiva(o)) {
                        o.HistorialesNormaSuscrita = await historialNormaSuscritaBcp.ObtenerVigentesPorNormaSuscrita(o.Id, transaction);
                    } else {
                        o.HistorialesNormaSuscrita = await historialNormaSuscritaBcp.ObtenerVigentesPorNormaSuscritaCompletadas(o.Id, transaction);
                    }
                    return o;
                })
            )];

            return obligaciones;
		}

		public async Task ActualizarProgramacionProcesosNormaSuscrita(long idNormaSuscrita, NpgsqlTransaction? transaction = null) {
			List<ProcesoNotificacion> procesosProgramados = [];
			List<ProcesoNotificacion> procesosDesprogramados = [];
			try {
				NormaSuscrita? normaSuscrita = await ObtenerConTemplate(idNormaSuscrita, transaction) ?? throw new InvalidOperationException("Norma suscrita inválida");
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

        public async Task<NormaSuscrita> CrearNormaSuscrita(string sub, long idNegocio, string nombre, string? descripcion, string? multa, long? idTipoPeriodicidad, long? idCategoriaNorma, long? idCargo, bool activado, DateTime? proximoVencimiento, HashSet<long> idFiscalizadores, HashSet<(long IdTipoUnidadTiempo, int CantAntelacion)> antelaciones, IDatabaseTransaction? transaction = null) {
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
                await ActualizarProgramacionProcesosNormaSuscrita(obligacion.Id, transaction!.NpgsqlTransaction());

                if (ownsTransaction) {
                    await transaction!.CommitAsync();
                }

                return obligacion;
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

		public async Task<HistorialNormaSuscrita> CompletarNormaValidandoPertenencia(string sub, long idNormaSuscrita, long idHistorialNormaSuscrita, IDatabaseTransaction? transaction = null) {
            bool ownsTransaction = transaction == null;
            IDatabaseConnection? connection = null;
            try {
                if (ownsTransaction) {
                    connection = await connectionHelper.ObtenerConexionWrapper();
                    transaction = await connection.BeginTransactionAsync();
                }

                NormaSuscrita obligacion = await normaSuscritaBcp.ObtenerValidandoVigenciaYPertenencia(idNormaSuscrita, sub, transaction!.NpgsqlTransaction());
                HistorialNormaSuscrita vencimiento = await historialNormaSuscritaBcp.ObtenerValidandoVigenciaYPertenencia(idHistorialNormaSuscrita, idNormaSuscrita, transaction!.NpgsqlTransaction());

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
				HistorialNormaSuscrita vencimiento = await historialNormaSuscritaBcp.ObtenerValidandoVigencia(historialNotificacion.IdHistorialNormaSuscrita, transaction!.NpgsqlTransaction());
				NormaSuscrita obligacion = await normaSuscritaBcp.ObtenerValidandoVigencia(vencimiento.IdNormaSuscrita, transaction!.NpgsqlTransaction());

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
	}
}
