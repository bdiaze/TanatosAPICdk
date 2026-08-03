using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Npgsql;
using System.Collections.Specialized;
using System.Globalization;
using System.Text.Json;
using System.Transactions;
using TanatosAPI.Entities.Models;
using TanatosAPI.Entities.Others.Kairos;
using TanatosAPI.Exceptions;
using TanatosAPI.Helpers;
using TanatosAPI.Interfaces.Business;
using TanatosAPI.Interfaces.Helpers;
using TanatosAPI.Interfaces.Repositories;
using TanatosAPI.Repositories;

namespace TanatosAPI.Business {
	public class NormaSuscritaBcp(IDateTimeProvider dateTimeProvider, IVariableEntornoHelper variableEntornoHelper, IKairosHelper kairosHelper, INormaSuscritaDao normaSuscritaDao) : INormaSuscritaBcp {
		public bool EstaVigente(NormaSuscrita? normaSuscrita) {
			return normaSuscrita != null && normaSuscrita.Vigencia;
		}

		public bool Pertenece(NormaSuscrita normaSuscrita, string sub) {
			return normaSuscrita.Sub == sub;
		}

		public bool PerteneceNegocio(NormaSuscrita normaSuscrita, long idNegocio) {
			return normaSuscrita.IdNegocio == idNegocio;
		}

		public bool EstaActiva(NormaSuscrita normaSuscrita) {
			return EstaVigente(normaSuscrita) && normaSuscrita.Activado;
		}

        public bool EsEditable(NormaSuscrita normaSuscrita) {
            return normaSuscrita.Editable;
        }

		public List<NormaSuscrita> FiltrarVigentes(List<NormaSuscrita> normasSuscritas) {
			return [.. normasSuscritas.Where(ns => EstaVigente(ns))];
		}

		public async Task<NormaSuscrita?> Obtener(long idNormaSuscrita, bool filtrarVigente = false, bool validarVigencia = false, string? validarSub = null, long? validarIdNegocio = null, bool validarEditable = false, NpgsqlTransaction? transaction = null) {
			NormaSuscrita? normaSuscrita = await normaSuscritaDao.ObtenerPorId(idNormaSuscrita, transaction);
			// Se aplican todas las validaciones...
			if (validarVigencia && !EstaVigente(normaSuscrita)) throw new ErrorValidacion(TipoErrorValidacion.NoVigente, "La obligación no existe o no está vigente", "La obligación es inválida.");
			if (normaSuscrita != null) {
				if (validarSub != null && !Pertenece(normaSuscrita, validarSub)) throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "La obligación no pertenece al usuario", "La obligación es inválida.");
				if (validarIdNegocio != null && !PerteneceNegocio(normaSuscrita, validarIdNegocio.Value)) throw new ErrorValidacion(TipoErrorValidacion.NoPertenece, "La obligación no pertenece al negocio", "La obligación es inválida.");
				if (validarEditable && !EsEditable(normaSuscrita)) throw new ErrorValidacion(TipoErrorValidacion.EstadoNoValido, "La obligación no es editable por el usuario", "La obligación es inválida.");
			}

			// Se aplican los filtros...
			if (filtrarVigente && !EstaVigente(normaSuscrita)) return null;

			return normaSuscrita;
		}

		public async Task<List<NormaSuscrita>> ObtenerPorSubYNegocio(string sub, long idNegocio, bool filtrarVigentes = false, NpgsqlTransaction? transaction = null) {
			List<NormaSuscrita> normasSuscritas = await normaSuscritaDao.ObtenerPorSub(sub, idNegocio, null, transaction);
			if (filtrarVigentes) normasSuscritas = FiltrarVigentes(normasSuscritas);
			return normasSuscritas;
		}
				
		public async Task<NormaSuscrita> CrearObligacionUsuario(string sub, long idNegocio, string nombre, string? descripcion, string? multa, long? idTipoPeriodicidad, long? idCategoriaNorma, long? idCargo, bool activado, NpgsqlTransaction? transaction = null) {
            nombre = nombre.Trim();
            descripcion = string.IsNullOrWhiteSpace(descripcion) ? null : descripcion.Trim();
            multa = string.IsNullOrWhiteSpace(multa) ? null : multa.Trim();

			// Se valida que no exista otra obligación con el mismo nombre...
			List<NormaSuscrita> vigentes = await ObtenerPorSubYNegocio(sub, idNegocio, filtrarVigentes: true);
			if (vigentes.Any(o => o.Nombre == nombre)) throw new ErrorValidacion(TipoErrorValidacion.YaExiste, "Ya existe una obligación con dicho nombre."); 

            DateTime now = dateTimeProvider.UtcNow;
            NormaSuscrita nuevo = new() {
                Id = 0,
                Sub = sub,
                IdNegocio = idNegocio,
                IdTemplate = null,
                IdNorma = null,
                Nombre = nombre,
                Descripcion = descripcion,
                Multa = multa,
                IdTipoPeriodicidad = idTipoPeriodicidad,
                IdCategoriaNorma = idCategoriaNorma,
                IdCargo = idCargo,
                OrdenVisual = null,
                Editable = true,
                FechaActivacion = activado ? now : null,
                FechaDesactivacion = null,
                Activado = activado,
                FechaCreacion = now,
                FechaEliminacion = null,
                Vigencia = true
            };
            nuevo.Id = await normaSuscritaDao.Insertar(nuevo, transaction);
			return nuevo;
        }

		public async Task Actualizar(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null) {
			await normaSuscritaDao.Actualizar(normaSuscrita, transaction);
		}

		public async Task Activar(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null) {
			if (!normaSuscrita.Activado) {
				normaSuscrita.FechaActivacion = dateTimeProvider.UtcNow;
				normaSuscrita.FechaDesactivacion = null;
				normaSuscrita.Activado = true;

				await normaSuscritaDao.Actualizar(normaSuscrita, transaction);
			}
		}

		public async Task Desactivar(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null) {
			if (normaSuscrita.Activado) {
                normaSuscrita.FechaDesactivacion = dateTimeProvider.UtcNow;
                normaSuscrita.Activado = false;

				await normaSuscritaDao.Actualizar(normaSuscrita, transaction);
            }
		}

		public async Task Eliminar(NormaSuscrita normaSuscrita, NpgsqlTransaction? transaction = null) {
			if (normaSuscrita.Vigencia) {
				await Desactivar(normaSuscrita, transaction);

                normaSuscrita.FechaEliminacion = dateTimeProvider.UtcNow;
                normaSuscrita.Vigencia = false;

                await normaSuscritaDao.Actualizar(normaSuscrita, transaction);
            }
		}

		public async Task ProgramarUnProcesoNotificacion(EntKairosIngresarProceso procesosProgramar) {
			await kairosHelper.IngresarProceso(procesosProgramar);
		}

		public async Task ProgramarVariosProcesosNotificacion(List<EntKairosIngresarProceso> procesosProgramar) {
			foreach (EntKairosIngresarProceso proceso in procesosProgramar) {
				await ProgramarUnProcesoNotificacion(proceso);
			}
		}

		public async Task DesprogramarUnProcesoNotificacion(string idProcesosDesprogramar) {
			await kairosHelper.EliminarProceso(idProcesosDesprogramar);
		}

		public async Task DesprogramarVariosProcesosNotificacion(List<string> idProcesosDesprogramar) {
			foreach (string idProceso in idProcesosDesprogramar) {
				await DesprogramarUnProcesoNotificacion(idProceso);
			}
		}
		
		public async Task ReversarProcesos(List<ProcesoNotificacion> procesosProgramados, List<ProcesoNotificacion> procesosDesprogramados) {
            await ProgramarVariosProcesosNotificacion([.. procesosDesprogramados.Select(p => new EntKairosIngresarProceso() {
				Nombre = p.Nombre,
				Cron = p.Cron,
				FrecuenciaDias = p.FrecuenciaDias,
				InicioEjecucionUtc = p.InicioEjecucionUtc,
				ArnRol = p.ArnRol,
				ArnProceso = p.ArnProceso,
				Parametros = p.Parametros,
				Habilitado = p.Habilitado
			})]);
            await DesprogramarVariosProcesosNotificacion([.. procesosProgramados.Select(p => p.IdProceso)]);
        }

		public List<ProcesoNotificacion> ExtraerCronsAEliminar(NormaSuscrita normaSuscrita, List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> cronsDeseados) {
			HashSet<(string Cron, long? IdUnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> deseados = [.. cronsDeseados.Select(c => (c.Cron, c.UnidadTiempoAntelacion?.Id, c.CantAntelacion, c.EsVencimiento))];

            List<ProcesoNotificacion> aEliminar = [];
			foreach (ProcesoNotificacion existente in normaSuscrita.ProcesosNotificaciones.Where(p => p.Cron != null)) {
				EntKairosParametrosProceso parametros = JsonSerializer.Deserialize(existente.Parametros, AppJsonSerializerContext.Default.EntKairosParametrosProceso)!;
                if (!deseados.Contains((existente.Cron!, parametros.IdTipoUnidadTiempoAntelacion, parametros.CantAntelacion, parametros.EsVencimiento ?? false))) {
					aEliminar.Add(existente);
				}
			}
			return aEliminar;
        }

		public List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> ExtraerCronsACrear(NormaSuscrita normaSuscrita, List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> cronsDeseados) {
			HashSet<(string Cron, long? IdUnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> existentes = [.. normaSuscrita.ProcesosNotificaciones
				.Where(p => p.Cron != null)
				.Select(p => {
					EntKairosParametrosProceso parametros = JsonSerializer.Deserialize(p.Parametros, AppJsonSerializerContext.Default.EntKairosParametrosProceso)!;
					return (p.Cron!, parametros.IdTipoUnidadTiempoAntelacion, parametros.CantAntelacion, parametros.EsVencimiento ?? false);
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

        public async Task<(List<ProcesoNotificacion> procesosCronProgramados, List<ProcesoNotificacion> procesosCronDesprogramados)> ActualizarProcesosCronProgramados(NormaSuscrita normaSuscrita, List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> cronsDeseados, NpgsqlTransaction? transaction = null) {
			List<ProcesoNotificacion> procesosProgramados = [];
			List<ProcesoNotificacion> procesosDesprogramados = [];

			try {
				List<ProcesoNotificacion> cronsAEliminar = ExtraerCronsAEliminar(normaSuscrita, cronsDeseados);
				List<(string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> cronsACrear = ExtraerCronsACrear(normaSuscrita, cronsDeseados);

				foreach (ProcesoNotificacion eliminar in cronsAEliminar) {
					await DesprogramarUnProcesoNotificacion(eliminar.IdProceso);
					procesosDesprogramados.Add(eliminar);

					normaSuscrita.ProcesosNotificaciones.RemoveAll(p => p.IdProceso == eliminar.IdProceso);
				}

				foreach ((string Cron, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento) crear in cronsACrear) {
					EntKairosParametrosProceso parametros = new() {
						IdNormaSuscrita = normaSuscrita.Id,
						Cron = crear.Cron,
						IdTipoUnidadTiempoAntelacion = crear.UnidadTiempoAntelacion?.Id,
						CantAntelacion = crear.CantAntelacion,
						EsVencimiento = crear.EsVencimiento,
						ProgramarSiguienteEjecucion = crear.EsVencimiento
					};

					SalKairosIngresarProceso retorno = await kairosHelper.IngresarProceso(new EntKairosIngresarProceso {
						Nombre = $"{variableEntornoHelper.Obtener("APP_NAME")} - NormaSuscrita {normaSuscrita.Id} - Cron {crear.Cron}",
						Cron = crear.Cron,
						Parametros = JsonSerializer.Serialize(parametros, AppJsonSerializerContext.Default.EntKairosParametrosProceso),
						ArnProceso = variableEntornoHelper.Obtener("NOTIFICACIONES_LAMBDA_ARN"),
						ArnRol = variableEntornoHelper.Obtener("NOTIFICACIONES_EJECUCION_ROLE_ARN"),
						Habilitado = true,
					});
					ProcesoNotificacion proceso = new() { 
						IdProceso = retorno.IdProceso,
						IdCalendarizacion = retorno.IdCalendarizacion,
						Nombre = retorno.Nombre,
						ArnRol = retorno.ArnRol,
						ArnProceso = retorno.ArnProceso,
						Parametros = retorno.Parametros,
						Habilitado = retorno.Habilitado,
						FechaCreacion = retorno.FechaCreacion,
						Cron = crear.Cron
					};
					procesosProgramados.Add(proceso);
					normaSuscrita.ProcesosNotificaciones.Add(proceso);
				}

				await Actualizar(normaSuscrita, transaction);
			} catch {
                await ReversarProcesos(procesosProgramados, procesosDesprogramados);
                throw;
            }
            return (procesosProgramados, procesosDesprogramados);
		}

        public List<ProcesoNotificacion> ExtraerFrecuenciasDiasAEliminar(NormaSuscrita normaSuscrita, List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> frecuenciasDiasDeseadas) {
            HashSet<(int FrecuenciaDias, DateTime InicioEjecucionUtc, long? IdUnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> deseados = [.. frecuenciasDiasDeseadas.Select(c => (c.FrecuenciaDias, c.InicioEjecucionUtc, c.UnidadTiempoAntelacion?.Id, c.CantAntelacion, c.EsVencimiento))];

            List<ProcesoNotificacion> aEliminar = [];
            foreach (ProcesoNotificacion existente in normaSuscrita.ProcesosNotificaciones.Where(p => p.FrecuenciaDias != null && p.InicioEjecucionUtc != null)) {
                EntKairosParametrosProceso parametros = JsonSerializer.Deserialize(existente.Parametros, AppJsonSerializerContext.Default.EntKairosParametrosProceso)!;
                if (!deseados.Contains((existente.FrecuenciaDias!.Value, existente.InicioEjecucionUtc!.Value, parametros.IdTipoUnidadTiempoAntelacion, parametros.CantAntelacion, parametros.EsVencimiento ?? false))) {
                    aEliminar.Add(existente);
                }
            }
            return aEliminar;
        }

        public List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> ExtraerFrecuenciasDiasACrear(NormaSuscrita normaSuscrita, List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> frecuenciasDiasDeseadas) {
            HashSet<(int FrecuenciaDias, DateTime InicioEjecucionUtc, long? IdUnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> existentes = [.. normaSuscrita.ProcesosNotificaciones
                .Where(p => p.FrecuenciaDias != null && p.InicioEjecucionUtc != null)
                .Select(p => {
                    EntKairosParametrosProceso parametros = JsonSerializer.Deserialize(p.Parametros, AppJsonSerializerContext.Default.EntKairosParametrosProceso)!;
                    return (p.FrecuenciaDias!.Value, p.InicioEjecucionUtc!.Value, parametros.IdTipoUnidadTiempoAntelacion, parametros.CantAntelacion, parametros.EsVencimiento ?? false);
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

        public async Task<(List<ProcesoNotificacion> frecuenciasDiasProgramados, List<ProcesoNotificacion> frecuenciasDiasDesprogramadas)> ActualizarProcesosFrecuenciaDiasProgramados(NormaSuscrita normaSuscrita, List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> frecuenciasDiasDeseadas, NpgsqlTransaction? transaction = null) {
            List<ProcesoNotificacion> procesosProgramados = [];
            List<ProcesoNotificacion> procesosDesprogramados = [];

            try {
                List<ProcesoNotificacion> cronsAEliminar = ExtraerFrecuenciasDiasAEliminar(normaSuscrita, frecuenciasDiasDeseadas);
                List<(int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento)> cronsACrear = ExtraerFrecuenciasDiasACrear(normaSuscrita, frecuenciasDiasDeseadas);

                foreach (ProcesoNotificacion eliminar in cronsAEliminar) {
                    await DesprogramarUnProcesoNotificacion(eliminar.IdProceso);
                    procesosDesprogramados.Add(eliminar);

                    normaSuscrita.ProcesosNotificaciones.RemoveAll(p => p.IdProceso == eliminar.IdProceso);
                }

                foreach ((int FrecuenciaDias, DateTime InicioEjecucionUtc, TipoUnidadTiempo? UnidadTiempoAntelacion, int? CantAntelacion, bool EsVencimiento) crear in cronsACrear) {
                    EntKairosParametrosProceso parametros = new() {
                        IdNormaSuscrita = normaSuscrita.Id,
						FrecuenciaDias = crear.FrecuenciaDias,
						InicioEjecucionUtc = crear.InicioEjecucionUtc,
                        IdTipoUnidadTiempoAntelacion = crear.UnidadTiempoAntelacion?.Id,
                        CantAntelacion = crear.CantAntelacion,
                        EsVencimiento = crear.EsVencimiento,
                        ProgramarSiguienteEjecucion = crear.EsVencimiento
                    };


					DateTime inicioEjecucionChile = DateTimeHelper.TransformarFechaUTCATimezone(crear.InicioEjecucionUtc);
                    SalKairosIngresarProceso retorno = await kairosHelper.IngresarProceso(new EntKairosIngresarProceso {
						Nombre = $"{variableEntornoHelper.Obtener("APP_NAME")} - NormaSuscrita {normaSuscrita.Id} - Inicio {inicioEjecucionChile:dd-MM-yyyy HH:mm} - Frecuencia {crear.FrecuenciaDias} Días",
						FrecuenciaDias = crear.FrecuenciaDias,
						InicioEjecucionUtc = crear.InicioEjecucionUtc,
                        Parametros = JsonSerializer.Serialize(parametros, AppJsonSerializerContext.Default.EntKairosParametrosProceso),
                        ArnProceso = variableEntornoHelper.Obtener("NOTIFICACIONES_LAMBDA_ARN"),
                        ArnRol = variableEntornoHelper.Obtener("NOTIFICACIONES_EJECUCION_ROLE_ARN"),
                        Habilitado = true,
                    });
                    ProcesoNotificacion proceso = new() {
                        IdProceso = retorno.IdProceso,
                        IdCalendarizacion = retorno.IdCalendarizacion,
                        Nombre = retorno.Nombre,
                        ArnRol = retorno.ArnRol,
                        ArnProceso = retorno.ArnProceso,
                        Parametros = retorno.Parametros,
                        Habilitado = retorno.Habilitado,
                        FechaCreacion = retorno.FechaCreacion,
						FrecuenciaDias = crear.FrecuenciaDias,
						InicioEjecucionUtc = crear.InicioEjecucionUtc
                    };
                    procesosProgramados.Add(proceso);
                    normaSuscrita.ProcesosNotificaciones.Add(proceso);
                }

                await Actualizar(normaSuscrita, transaction);
            } catch {
                await ReversarProcesos(procesosProgramados, procesosDesprogramados);
                throw;
            }
            return (procesosProgramados, procesosDesprogramados);
        }
    }
}
